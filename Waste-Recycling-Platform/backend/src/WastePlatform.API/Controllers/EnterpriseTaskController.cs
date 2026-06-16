using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WastePlatform.Application.Complaints.Queries;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.SignalR;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/enterprise/tasks")]
[Authorize(Roles = "Admin,Enterprise")] // Đã sửa để cho phép cả Admin
public class EnterpriseTaskController : ControllerBase
{
    private readonly WastePlatformDbContext _context;
    private readonly IHubContext<TaskHub> _hubContext;
    private readonly INotificationService _notificationService;
    private readonly IMediator _mediator;

    public EnterpriseTaskController(WastePlatformDbContext context, IHubContext<TaskHub> hubContext, INotificationService notificationService, IMediator mediator)
    {
        _context = context;
        _hubContext = hubContext;
        _notificationService = notificationService;
        _mediator = mediator;
    }

    private async Task<Enterprise?> GetCurrentEnterpriseAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return null;

        return await _context.Enterprises
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.UserId == userId);
    }

    /// <summary>
    /// Lấy danh sách nhiệm vụ thu gom của Enterprise (có thể chưa gán Collector)
    /// Admin có thể xem tất cả
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTasks([FromQuery] CollectionTaskStatus? status = null, [FromQuery] bool? unassigned = null)
    {
        bool isAdmin = User.IsInRole("Admin");
        Enterprise? enterprise = null;

        if (!isAdmin)
        {
            enterprise = await GetCurrentEnterpriseAsync();
            if (enterprise == null)
                return Unauthorized(new { message = "Enterprise profile not found for current user." });
        }

        var query = _context.CollectionTasks
            .Include(t => t.WasteReport)
                .ThenInclude(r => r.WasteCategory)
            .Include(t => t.WasteReport)
                .ThenInclude(r => r.Citizen)
            .Include(t => t.Collector!)
                .ThenInclude(c => c.User)
            .Include(t => t.Images)
            .Include(t => t.StatusLogs)
            .AsQueryable();

        // Nếu KHÔNG phải Admin thì chỉ lấy task của Enterprise đó
        if (!isAdmin && enterprise != null)
        {
            query = query.Where(t => t.EnterpriseId == enterprise.Id);
        }

        // Filter by unassigned status if requested
        if (unassigned == true)
        {
            query = query.Where(t => t.CollectorId == null);
        }

        // Filter by status if provided
        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        var tasks = await query
            .OrderByDescending(t => t.AssignedAt)
            .Select(t => new
            {
                t.Id,
                t.ReportId,
                t.EnterpriseId,
                t.CollectorId,
                CollectorName = t.Collector != null ? t.Collector.User.FullName : null,
                CollectorPhone = t.Collector != null ? t.Collector.User.Phone : null,
                Status = t.Status.ToString(),
                t.CollectedWeightKg,
                t.Notes,
                t.AssignedAt,
                t.CompletedAt,
                LatestStatusChangedAt = t.StatusLogs
                    .OrderByDescending(log => log.ChangedAt)
                    .Select(log => (DateTime?)log.ChangedAt)
                    .FirstOrDefault(),
                Report = new
                {
                    t.WasteReport.Id,
                    t.WasteReport.Description,
                    t.WasteReport.Address,
                    t.WasteReport.Latitude,
                    t.WasteReport.Longitude,
                    Status = t.WasteReport.Status.ToString(),
                    CategoryName = t.WasteReport.WasteCategory != null ? t.WasteReport.WasteCategory.Name : null,
                    CitizenName = t.WasteReport.Citizen.FullName,
                    CitizenPhone = t.WasteReport.Citizen.Phone
                }
            })
            .ToListAsync();

        return Ok(tasks);
    }

    /// <summary>
    /// Gán Collector cho một nhiệm vụ thu gom
    /// </summary>
    [HttpPut("{id}/assign-collector")]
    public async Task<IActionResult> AssignCollector([Required] Guid id, [FromBody] AssignCollectorRequest request)
    {
        bool isAdmin = User.IsInRole("Admin");
        Enterprise? enterprise = null;

        if (!isAdmin)
        {
            enterprise = await GetCurrentEnterpriseAsync();
            if (enterprise == null)
                return Unauthorized(new { message = "Enterprise profile not found." });
        }

        if (request.CollectorId == Guid.Empty)
            return BadRequest(new { message = "CollectorId is required." });

        var taskQuery = _context.CollectionTasks.Where(t => t.Id == id);
        if (!isAdmin && enterprise != null)
        {
            taskQuery = taskQuery.Where(t => t.EnterpriseId == enterprise.Id);
        }

        var task = await taskQuery.FirstOrDefaultAsync();
        if (task == null)
            return NotFound(new { message = "Task not found or does not belong to your enterprise." });

        // Verify collector belongs to this enterprise
        var collectorQuery = _context.Collectors.Where(c => c.Id == request.CollectorId);
        if (!isAdmin && enterprise != null)
        {
            collectorQuery = collectorQuery.Where(c => c.EnterpriseId == enterprise.Id);
        }

        var collector = await collectorQuery.FirstOrDefaultAsync();
        if (collector == null)
            return BadRequest(new { message = "Collector not found or does not belong to your enterprise." });

        try
        {
            task.AssignCollector(request.CollectorId);
            await _context.SaveChangesAsync();

            // Phát sóng sự kiện SignalR tới toàn bộ Client
            await _hubContext.Clients.All.SendAsync("TaskStatusUpdated", id, CollectionTaskStatus.Assigned.ToString());

            // Gửi thông báo: Report được phân công (Trigger #3 - Accepted → Assigned)
            var taskWithReport = await _context.CollectionTasks
                .Include(t => t.WasteReport)
                .Include(t => t.Collector)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (taskWithReport?.Collector != null)
            {
                await _notificationService.NotifyReportAssignedAsync(
                    taskWithReport.WasteReport.CitizenId, 
                    taskWithReport.ReportId, 
                    taskWithReport.Collector.User.FullName);
            }

            return Ok(new 
            { 
                message = "Collector assigned successfully.", 
                taskId = id,
                collectorId = request.CollectorId
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy danh sách Collectors của Enterprise (có thể gán công việc)
    /// Admin lấy toàn bộ
    /// </summary>
    [HttpGet("collectors")]
    public async Task<IActionResult> GetAvailableCollectors()
    {
        bool isAdmin = User.IsInRole("Admin");
        Enterprise? enterprise = null;

        if (!isAdmin)
        {
            enterprise = await GetCurrentEnterpriseAsync();
            if (enterprise == null)
                return Unauthorized(new { message = "Enterprise profile not found." });
        }

        var query = _context.Collectors
            .Include(c => c.User)
            .AsQueryable();

        if (!isAdmin && enterprise != null)
        {
            query = query.Where(c => c.EnterpriseId == enterprise.Id);
        }

        var collectors = await query
            .OrderBy(c => c.User.FullName)
            .Select(c => new
            {
                c.Id,
                Name = c.User.FullName,
                Email = c.User.Email,
                Phone = c.User.Phone,
                c.IsAvailable,
                c.CreatedAt,
                TaskCount = c.CollectionTasks.Count(t => t.Status != CollectionTaskStatus.Collected)
            })
            .ToListAsync();

        return Ok(collectors);
    }

    /// <summary>
    /// Lấy hồ sơ doanh nghiệp và các loại rác đang tiếp nhận
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // Admin không có Profile Enterprise nên sẽ báo lỗi hợp lý
        if (User.IsInRole("Admin"))
            return BadRequest(new { message = "Admin users do not have an enterprise profile." });

        var enterprise = await GetCurrentEnterpriseAsync();
        if (enterprise == null)
            return Unauthorized(new { message = "Enterprise profile not found." });

        var acceptedWasteTypes = await _context.EnterpriseWasteTypes
            .Where(wt => wt.EnterpriseId == enterprise.Id)
            .Include(wt => wt.WasteCategory)
            .Select(wt => new
            {
                wt.WasteCategoryId,
                CategoryName = wt.WasteCategory.Name
            })
            .ToListAsync();

        return Ok(new
        {
            enterprise.Id,
            enterprise.CompanyName,
            enterprise.ServiceArea,
            enterprise.CapacityKgPerDay,
            enterprise.Status,
            enterprise.RejectionReason,
            AcceptedWasteTypes = acceptedWasteTypes
        });
    }

    /// <summary>
    /// Cập nhật thông tin năng lực xử lý rác của Enterprise
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateEnterpriseProfileRequest request)
    {
        if (User.IsInRole("Admin"))
            return BadRequest(new { message = "Admin users cannot update enterprise profiles directly here." });

        var enterprise = await GetCurrentEnterpriseAsync();
        if (enterprise == null)
            return Unauthorized(new { message = "Enterprise profile not found." });

        enterprise.ServiceArea = string.IsNullOrWhiteSpace(request.ServiceArea) ? null : request.ServiceArea.Trim();
        enterprise.CapacityKgPerDay = request.CapacityKgPerDay;

        // CHỈ đổi trạng thái thành Pending nếu họ đang bị "Từ chối" (để xin duyệt lại).
        // Còn nếu đã "Verified" (Đã duyệt) thì cho họ sửa thoải mái, không bắt duyệt lại!
        if (enterprise.Status == "Rejected")
        {
            enterprise.Status = "Pending";
        }

        _context.Enterprises.Update(enterprise);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Enterprise profile updated successfully",
            enterprise.Id,
            enterprise.ServiceArea,
            enterprise.CapacityKgPerDay,
            Status = enterprise.Status,
            enterprise.RejectionReason
        });
    }

    /// <summary>
    /// Lấy danh sách loại rác có thể xử lý và các loại rác đang được lựa chọn
    /// </summary>
    [HttpGet("waste-types")]
    public async Task<IActionResult> GetWasteTypes()
    {
        if (User.IsInRole("Admin"))
            return BadRequest(new { message = "Admin users do not have enterprise waste types." });

        var enterprise = await GetCurrentEnterpriseAsync();
        if (enterprise == null)
            return Unauthorized(new { message = "Enterprise profile not found." });

        var allCategories = await _context.WasteCategories
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Name
            })
            .ToListAsync();

        var acceptedIds = await _context.EnterpriseWasteTypes
            .Where(wt => wt.EnterpriseId == enterprise.Id)
            .Select(wt => wt.WasteCategoryId)
            .ToListAsync();

        return Ok(new
        {
            allCategories,
            acceptedIds
        });
    }

    /// <summary>
    /// Cập nhật danh sách loại rác Enterprise tiếp nhận
    /// </summary>
    [HttpPut("waste-types")]
    public async Task<IActionResult> UpdateWasteTypes([FromBody] UpdateEnterpriseWasteTypesRequest request)
    {
        if (User.IsInRole("Admin"))
            return BadRequest(new { message = "Admin users cannot update waste types." });

        var enterprise = await GetCurrentEnterpriseAsync();
        if (enterprise == null)
            return Unauthorized(new { message = "Enterprise profile not found." });

        var validCategoryIds = await _context.WasteCategories
            .Where(c => request.WasteCategoryIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        if (validCategoryIds.Count != request.WasteCategoryIds.Distinct().Count())
            return BadRequest(new { message = "One or more selected waste categories are invalid." });

        var existingTypes = await _context.EnterpriseWasteTypes
            .Where(wt => wt.EnterpriseId == enterprise.Id)
            .ToListAsync();

        var existingIds = existingTypes.Select(wt => wt.WasteCategoryId).ToHashSet();
        var toRemove = existingTypes.Where(wt => !validCategoryIds.Contains(wt.WasteCategoryId)).ToList();
        _context.EnterpriseWasteTypes.RemoveRange(toRemove);

        var toAdd = validCategoryIds.Except(existingIds)
            .Select(id => new EnterpriseWasteType
            {
                EnterpriseId = enterprise.Id,
                WasteCategoryId = id
            });

        await _context.EnterpriseWasteTypes.AddRangeAsync(toAdd);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Accepted waste categories updated successfully",
            acceptedIds = validCategoryIds
        });
    }

    private static IEnumerable<string> ParseServiceAreaValues(string? serviceArea)
    {
        if (string.IsNullOrWhiteSpace(serviceArea))
            return Array.Empty<string>();

        try
        {
            using var document = JsonDocument.Parse(serviceArea);
            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                return document.RootElement.EnumerateArray()
                    .Where(e => e.ValueKind == JsonValueKind.String)
                    .Select(e => e.GetString()!.Trim())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .ToList();
            }

            if (document.RootElement.ValueKind == JsonValueKind.String)
            {
                var value = document.RootElement.GetString();
                return string.IsNullOrWhiteSpace(value)
                    ? Array.Empty<string>()
                    : new[] { value.Trim() };
            }
        }
        catch (JsonException)
        {
            // not valid JSON, fallback to comma-separated text
        }

        return serviceArea.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();
    }

    private static bool IsReportInServiceArea(WasteReport report, IEnumerable<string> serviceAreaTerms)
    {
        var terms = serviceAreaTerms.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        if (!terms.Any())
            return true;

        if (!string.IsNullOrWhiteSpace(report.Address))
        {
            return terms.Any(term => report.Address.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    /// <summary>
    /// Lấy thống kê công việc của Enterprise
    /// Admin sẽ lấy thống kê của toàn bộ hệ thống
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        bool isAdmin = User.IsInRole("Admin");
        Enterprise? enterprise = null;

        if (!isAdmin)
        {
            enterprise = await GetCurrentEnterpriseAsync();
            if (enterprise == null)
                return Unauthorized(new { message = "Enterprise profile not found." });
        }

        var query = _context.CollectionTasks.AsQueryable();

        if (!isAdmin && enterprise != null)
        {
            query = query.Where(t => t.EnterpriseId == enterprise.Id);
        }

        var tasks = await query.Include(t => t.WasteReport).ToListAsync();

        var totalUnassigned = tasks.Count(t => t.CollectorId == null);
        var totalAssigned = tasks.Count(t => t.Status == CollectionTaskStatus.Assigned);
        var totalOnTheWay = tasks.Count(t => t.Status == CollectionTaskStatus.OnTheWay);
        var totalCollected = tasks.Count(t => t.Status == CollectionTaskStatus.Collected);
        var totalWeight = tasks
            .Where(t => t.Status == CollectionTaskStatus.Collected && t.CollectedWeightKg.HasValue)
            .Sum(t => t.CollectedWeightKg!.Value);

        return Ok(new
        {
            TotalTasks = tasks.Count,
            TotalUnassigned = totalUnassigned,
            TotalAssigned = totalAssigned,
            TotalOnTheWay = totalOnTheWay,
            TotalCollected = totalCollected,
            TotalWeightKg = totalWeight
        });
    }

    /// <summary>
    /// Lấy tiến độ (timeline) chi tiết của một nhiệm vụ thu gom
    /// </summary>
    [HttpGet("{id}/progress")]
    public async Task<IActionResult> GetTaskProgress([Required] Guid id)
    {
        bool isAdmin = User.IsInRole("Admin");
        Enterprise? enterprise = null;

        if (!isAdmin)
        {
            enterprise = await GetCurrentEnterpriseAsync();
            if (enterprise == null)
                return Unauthorized(new { message = "Enterprise profile not found." });
        }

        var taskQuery = _context.CollectionTasks
            .Include(t => t.StatusLogs)
            .Include(t => t.Collector)
                .ThenInclude(c => c.User)
            .Include(t => t.Images)
            .Where(t => t.Id == id);

        if (!isAdmin && enterprise != null)
        {
            taskQuery = taskQuery.Where(t => t.EnterpriseId == enterprise.Id);
        }

        var task = await taskQuery.FirstOrDefaultAsync();
        if (task == null)
            return NotFound(new { message = "Task not found or does not belong to your enterprise." });

        var timeline = new List<TaskTimelineEventDto>();

        // Event 1: Assigned
        timeline.Add(new TaskTimelineEventDto
        {
            Status = CollectionTaskStatus.Assigned.ToString(),
            Timestamp = task.AssignedAt,
            Details = task.Collector != null 
                ? $"Được phân công cho người thu gom: {task.Collector.User.FullName}" 
                : "Task được khởi tạo, đang chờ phân công"
        });

        // Other events from StatusLogs
        var logs = task.StatusLogs.OrderBy(l => l.ChangedAt).ToList();
        foreach (var log in logs)
        {
            var eventDto = new TaskTimelineEventDto
            {
                Status = log.Status.ToString(),
                Timestamp = log.ChangedAt
            };

            if (log.Status == CollectionTaskStatus.OnTheWay)
            {
                eventDto.Details = "Người thu gom đang trên đường đến điểm lấy rác";
            }
            else if (log.Status == CollectionTaskStatus.Collected)
            {
                eventDto.Details = "Đã thu gom thành công";
                eventDto.CollectedWeightKg = task.CollectedWeightKg;
                eventDto.Notes = task.Notes;
                eventDto.Images = task.Images.Select(img => img.ImageUrl).ToList();
            }

            // prevent duplicate entry if it's somehow Assigned again (shouldn't happen per business logic but just in case)
            if (log.Status != CollectionTaskStatus.Assigned)
            {
                timeline.Add(eventDto);
            }
        }

        return Ok(new
        {
            TaskId = task.Id,
            CurrentStatus = task.Status.ToString(),
            Timeline = timeline.OrderBy(t => t.Timestamp).ToList()
        });
    }

    /// <summary>
    /// Lấy danh sách khiếu nại gửi đến Enterprise này
    /// </summary>
    [HttpGet("complaints")]
    public async Task<IActionResult> GetComplaints([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] ComplaintStatus? status = null)
    {
        try
        {
            var enterprise = await GetCurrentEnterpriseAsync();
            if (enterprise == null)
                return Unauthorized(new { message = "Enterprise profile not found." });

            var result = await _mediator.Send(new GetEnterpriseComplaintsQuery
            {
                EnterpriseId = enterprise.Id,
                Page = page,
                PageSize = pageSize,
                Status = status
            });

            return Ok(new
            {
                message = "Complaints retrieved successfully",
                data = result.Complaints,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalItems = result.Total,
                    totalPages = result.TotalPages
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Phản hồi và giải quyết khiếu nại
    /// </summary>
    [HttpPost("complaints/{id}/respond")]
    public async Task<IActionResult> RespondToComplaint([Required] Guid id, [FromBody] EnterpriseRespondRequest request)
    {
        try
        {
            var enterprise = await GetCurrentEnterpriseAsync();
            if (enterprise == null)
                return Unauthorized(new { message = "Enterprise profile not found." });

            var result = await _mediator.Send(new Application.Complaints.Commands.EnterpriseRespondToComplaintCommand
            {
                EnterpriseId = enterprise.Id,
                EnterpriseName = enterprise.CompanyName,
                ComplaintId = id,
                Response = request.Response,
                ResolveImmediately = request.ResolveImmediately,
                EscalateToAdmin = request.EscalateToAdmin
            });

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                complaintId = result.ComplaintId,
                status = result.NewStatus
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}

public class EnterpriseRespondRequest
{
    public string? Response { get; set; }
    public required bool ResolveImmediately { get; set; }
    public required bool EscalateToAdmin { get; set; }
}

/// <summary>
/// Request body for assigning a collector to a task
/// </summary>
public class AssignCollectorRequest
{
    public required Guid CollectorId { get; set; }
}

public class UpdateEnterpriseProfileRequest
{
    public string? ServiceArea { get; set; }
    public int? CapacityKgPerDay { get; set; }
}

public class UpdateEnterpriseWasteTypesRequest
{
    public List<int> WasteCategoryIds { get; set; } = new();
}

public class TaskTimelineEventDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
    public decimal? CollectedWeightKg { get; set; }
    public string? Notes { get; set; }
    public List<string> Images { get; set; } = new();
}