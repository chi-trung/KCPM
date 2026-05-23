using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Reports.Commands;
using WastePlatform.Application.Reports.Queries;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly WastePlatformDbContext _context;
    private readonly INotificationService _notificationService;

    public ReportController(IMediator mediator, WastePlatformDbContext context, INotificationService notificationService)
    {
        _mediator = mediator;
        _context = context;
        _notificationService = notificationService;
    }

    /// <summary>Tạo báo cáo rác mới</summary>
    [HttpPost("create")]
    [Authorize(Roles = "Citizen")]
    public async Task<IActionResult> CreateReport([FromForm] IFormCollection form)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Invalid or missing user ID in token" });

            if (!int.TryParse(form["WasteCategoryId"], out var categoryId))
                return BadRequest(new { message = "Invalid WasteCategoryId" });

            if (!decimal.TryParse(form["Latitude"], out var latitude))
                return BadRequest(new { message = "Invalid Latitude" });

            if (!decimal.TryParse(form["Longitude"], out var longitude))
                return BadRequest(new { message = "Invalid Longitude" });

            var command = new CreateReportCommand
            {
                CitizenId = userId,
                WasteCategoryId = categoryId,
                Latitude = latitude,
                Longitude = longitude,
                Description = form["Description"].ToString(),
                Address = form["Address"].ToString(),
                AiSuggestion = form["AiSuggestion"].ToString(),
                Images = form.Files
            };

            var reportId = await _mediator.Send(command);

            // Gửi thông báo: Báo cáo mới được tạo (Trigger #1)
            await _notificationService.NotifyReportCreatedAsync(userId, reportId);

            // Re-use GetReportByIdQuery to construct the response DTO natively
            var createdReportDto = await _mediator.Send(new GetReportByIdQuery { Id = reportId });

            var response = new
            {
                message = "Report created successfully",
                report = createdReportDto
            };

            return CreatedAtAction(nameof(GetReportById), new { id = reportId }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Lấy chi tiết báo cáo theo ID</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReportById(Guid id)
    {
        try
        {
            var report = await _mediator.Send(new GetReportByIdQuery { Id = id });

            if (report == null)
                return NotFound(new { message = "Report not found" });

            return Ok(new { message = "Report retrieved successfully", report = report });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Lấy danh sách báo cáo của người dùng hiện tại</summary>
    [HttpGet("my-reports")]
    public async Task<IActionResult> GetMyReports([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Invalid or missing user ID" });

            var result = await _mediator.Send(new GetMyReportsQuery 
            { 
                UserId = userId, 
                Page = page, 
                PageSize = pageSize 
            });

            var response = new
            {
                message = "Reports retrieved successfully",
                pagination = new
                {
                    page = page,
                    pageSize = pageSize,
                    total = result.Total,
                    totalPages = result.TotalPages
                },
                reports = result.Reports
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Lấy danh sách tất cả báo cáo (Admin/Enterprise)</summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin,Enterprise")]
    public async Task<IActionResult> GetAllReports([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
    {
        try
        {
            var result = await _mediator.Send(new GetAllReportsQuery 
            { 
                Page = page, 
                PageSize = pageSize, 
                Status = status 
            });

            var response = new
            {
                message = "All reports retrieved successfully",
                pagination = new
                {
                    page = page,
                    pageSize = pageSize,
                    total = result.Total,
                    totalPages = result.TotalPages
                },
                reports = result.Reports
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Chấp nhận báo cáo và tạo nhiệm vụ thu gom (Admin/Enterprise)</summary>
    [HttpPost("{id}/accept")]
    [Authorize(Roles = "Admin,Enterprise")]
    public async Task<IActionResult> AcceptReportAndCreateTask(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Invalid or missing user ID" });

            // Get the report
            var report = await _context.WasteReports.FindAsync(id);
            if (report == null)
                return NotFound(new { message = "Report not found" });

            // Check if report is in valid state
            if (report.Status != ReportStatus.Pending)
                return BadRequest(new { message = $"Report can only be accepted if it is in Pending status. Current status: {report.Status}" });

            // Update report status to Accepted
            report.Accept();
            
            // Xử lý riêng cho Role Enterprise
            if (roleClaim != null && roleClaim.Value == "Enterprise")
            {
                var enterprise = await _context.Enterprises.FirstOrDefaultAsync(e => e.UserId == userId);
                if (enterprise == null)
                    return Unauthorized(new { message = "Enterprise not found for current user" });

                var acceptedWasteCategoryIds = await _context.EnterpriseWasteTypes
                    .Where(ewt => ewt.EnterpriseId == enterprise.Id)
                    .Select(ewt => ewt.WasteCategoryId)
                    .ToListAsync();

                if (!report.WasteCategoryId.HasValue || !acceptedWasteCategoryIds.Contains(report.WasteCategoryId.Value))
                    return BadRequest(new { message = "This report's waste category is not handled by your enterprise." });

                var serviceAreaTerms = ParseServiceAreaValues(enterprise.ServiceArea);
                if (!IsReportInServiceArea(report, serviceAreaTerms))
                    return BadRequest(new { message = "This report is outside your enterprise service area." });

                // Create a collection task cho Enterprise
                var collectionTask = CollectionTask.Create(id, enterprise.Id);
                _context.CollectionTasks.Add(collectionTask);
            }
            else if (roleClaim != null && roleClaim.Value == "Admin")
            {
                // Nếu là Admin, chỉ đổi trạng thái báo cáo sang Accepted, không cần gán Enterprise ngay lập tức
                // Admin có thể gán thủ công ở một API khác, hoặc tính năng khác

                // DÀNH CHO ADMIN TEST: Tự động lấy Đại 1 Enterprise trong DB để gán Task
                var firstEnterprise = await _context.Enterprises.FirstOrDefaultAsync();
                if (firstEnterprise != null)
                {
                    var collectionTask = CollectionTask.Create(id, firstEnterprise.Id);
                    _context.CollectionTasks.Add(collectionTask);
                }
            }

            _context.WasteReports.Update(report);
            await _context.SaveChangesAsync();

            // Gửi thông báo: Report được chấp nhận (Trigger #2 - Pending → Accepted)
            await _notificationService.NotifyReportAcceptedAsync(report.CitizenId, report.Id);

            return Ok(new
            {
                message = "Report accepted successfully",
                reportId = id,
                reportStatus = report.Status.ToString()
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Từ chối báo cáo (Admin/Enterprise)</summary>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin,Enterprise")]
    public async Task<IActionResult> RejectReport(Guid id, [FromBody] RejectReportRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Invalid or missing user ID" });

            // Get the report
            var report = await _context.WasteReports.FindAsync(id);
            if (report == null)
                return NotFound(new { message = "Report not found" });

            // Check if report is in valid state
            if (report.Status != ReportStatus.Pending)
                return BadRequest(new { message = $"Report can only be rejected if it is in Pending status. Current status: {report.Status}" });

            // Update report status to Rejected
            report.Reject();

            // Lưu lý do từ chối (nếu bảng WasteReports của ông có chỗ lưu, hoặc chỉ cần trả về log)
            // Tạm thởi chỉ đổi status.

            _context.WasteReports.Update(report);
            await _context.SaveChangesAsync();

            // Gửi thông báo: Report bị từ chối (Trigger #6)
            await _notificationService.NotifyReportRejectedAsync(report.CitizenId, report.Id, request?.Reason);

            return Ok(new
            {
                message = "Report rejected successfully",
                reportId = id,
                reportStatus = report.Status.ToString(),
                rejectionReason = request?.Reason
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Lấy danh sách báo cáo rác mà doanh nghiệp có thể xử lý</summary>
    [HttpGet("enterprise/available")]
    [Authorize(Roles = "Enterprise")]
    public async Task<IActionResult> GetEnterpriseAvailableReports([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Invalid or missing user ID in token" });

            // Get enterprise for current user
            var enterprise = await _context.Enterprises
                .FirstOrDefaultAsync(e => e.UserId == userId);
            if (enterprise == null)
                return Unauthorized(new { message = "Enterprise profile not found for current user" });

            // Get reports available for this enterprise
            var result = await _mediator.Send(new GetEnterpriseReportsQuery
            {
                EnterpriseId = enterprise.Id,
                Page = page,
                PageSize = pageSize,
                Status = status
            });

            var response = new
            {
                message = "Available reports retrieved successfully",
                pagination = new
                {
                    page = page,
                    pageSize = pageSize,
                    total = result.Total,
                    totalPages = result.TotalPages
                },
                reports = result.Reports
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
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
}

public class RejectReportRequest
{
    public string? Reason { get; set; }
}