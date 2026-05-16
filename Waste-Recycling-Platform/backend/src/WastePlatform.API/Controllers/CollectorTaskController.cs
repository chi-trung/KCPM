using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WastePlatform.Application.Rewards.Commands;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using WastePlatform.Infrastructure.SignalR;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/collector/tasks")]
[Authorize(Roles = "Collector")]
public class CollectorTaskController : ControllerBase
{
    private readonly WastePlatformDbContext _context;
    private readonly IHubContext<TaskHub> _hubContext;
    private readonly IMediator _mediator;
    private readonly INotificationService _notificationService;

    public CollectorTaskController(
        WastePlatformDbContext context,
        IHubContext<TaskHub> hubContext,
        IMediator mediator,
        INotificationService notificationService)
    {
        _context = context;
        _hubContext = hubContext;
        _mediator = mediator;
        _notificationService = notificationService;
    }

    private async Task<Collector?> GetCurrentCollectorAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return null;

        return await _context.Collectors
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    /// <summary>
    /// Lấy danh sách nhiệm vụ thu gom của Collector hiện tại
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTasks([FromQuery] CollectionTaskStatus? status = null)
    {
        var collector = await GetCurrentCollectorAsync();
        if (collector == null)
            return Unauthorized(new { message = "Collector profile not found for current user." });

        var query = _context.CollectionTasks
            .Include(t => t.WasteReport)
                .ThenInclude(r => r.WasteCategory)
            .Include(t => t.WasteReport)
                .ThenInclude(r => r.Citizen)
            .Include(t => t.Images)
            .Where(t => t.CollectorId == collector.Id);

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
                Status = t.Status.ToString(),
                t.CollectedWeightKg,
                t.Notes,
                t.AssignedAt,
                t.CompletedAt,
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
    /// WRP-109: Lấy chi tiết một nhiệm vụ thu gom
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
        var collector = await GetCurrentCollectorAsync();
        if (collector == null)
            return Unauthorized(new { message = "Collector profile not found for current user." });

        var task = await _context.CollectionTasks
            .Include(t => t.WasteReport)
                .ThenInclude(r => r.WasteCategory)
            .Include(t => t.WasteReport)
                .ThenInclude(r => r.Citizen)
            .Include(t => t.WasteReport)
                .ThenInclude(r => r.Images)
            .Include(t => t.Images)
            .Include(t => t.StatusLogs)
            .FirstOrDefaultAsync(t => t.Id == id && t.CollectorId == collector.Id);

        if (task == null)
            return NotFound(new { message = "Task not found or not assigned to you." });

        return Ok(new
        {
            task.Id,
            task.ReportId,
            task.EnterpriseId,
            task.CollectorId,
            Status = task.Status.ToString(),
            task.CollectedWeightKg,
            task.Notes,
            task.AssignedAt,
            task.CompletedAt,
            Report = new
            {
                task.WasteReport.Id,
                task.WasteReport.Description,
                task.WasteReport.Address,
                task.WasteReport.Latitude,
                task.WasteReport.Longitude,
                Status = task.WasteReport.Status.ToString(),
                CategoryName = task.WasteReport.WasteCategory?.Name,
                CitizenName = task.WasteReport.Citizen?.FullName,
                CitizenPhone = task.WasteReport.Citizen?.Phone,
                ImageUrls = task.WasteReport.Images.Select(i => i.ImageUrl).ToList()
            },
            Images = task.Images.Select(i => i.ImageUrl),
            StatusLogs = task.StatusLogs.OrderByDescending(l => l.ChangedAt).Select(l => new 
            { 
                Status = l.Status.ToString(), 
                l.ChangedAt 
            })
        });
    }

    /// <summary>
    /// Cập nhật trạng thái nhiệm vụ thành "On the way" (Đang di chuyển)
    /// </summary>
    [HttpPut("{id}/on-the-way")]
    public async Task<IActionResult> SetOnTheWay(Guid id)
    {
        var collector = await GetCurrentCollectorAsync();
        if (collector == null)
            return Unauthorized(new { message = "Collector profile not found." });

        var task = await _context.CollectionTasks
            .Include(t => t.StatusLogs)
            .FirstOrDefaultAsync(t => t.Id == id && t.CollectorId == collector.Id);
        if (task == null)
            return NotFound(new { message = "Task not found or not assigned to you." });

        try
        {
            task.SetOnTheWay();
            
            // Force all existing logs to unchanged and new logs to added
            foreach(var entry in _context.ChangeTracker.Entries<TaskStatusLog>())
            {
                if (entry.Entity.Status == CollectionTaskStatus.OnTheWay && entry.Entity.Id != Guid.Empty && entry.State != EntityState.Unchanged)
                    entry.State = EntityState.Added;
                else if (entry.State == EntityState.Modified)
                    entry.State = EntityState.Unchanged;
            }

            await _context.SaveChangesAsync();
            
            // Phát sóng sự kiện SignalR tới toàn bộ Client
            await _hubContext.Clients.All.SendAsync("TaskStatusUpdated", id, CollectionTaskStatus.OnTheWay.ToString());

            // Gửi thông báo: Collector đang đến (Trigger #4)
            var taskWithReport = await _context.CollectionTasks
                .Include(t => t.WasteReport)
                .Include(t => t.Collector)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (taskWithReport?.Collector != null)
            {
                await _notificationService.NotifyCollectorOnTheWayAsync(
                    taskWithReport.WasteReport.CitizenId,
                    taskWithReport.ReportId,
                    taskWithReport.Collector.User.FullName);
            }

            return Ok(new { message = "Task status updated to OnTheWay.", taskId = id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật trạng thái nhiệm vụ thành "Collected" (Đã thu gom) kèm theo khối lượng, ghi chú và hình ảnh
    /// </summary>
    [HttpPut("{id}/complete")]
    public async Task<IActionResult> CompleteTask(Guid id, [FromForm] IFormCollection form)
    {
        var collector = await GetCurrentCollectorAsync();
        if (collector == null)
            return Unauthorized(new { message = "Collector profile not found." });

        var task = await _context.CollectionTasks
            .Include(t => t.WasteReport)
            .Include(t => t.StatusLogs)
            .Include(t => t.Images)
            .FirstOrDefaultAsync(t => t.Id == id && t.CollectorId == collector.Id);
            
        if (task == null)
            return NotFound(new { message = "Task not found or not assigned to you." });

        if (!decimal.TryParse(form["WeightKg"], out var weightKg))
            return BadRequest(new { message = "Invalid or missing WeightKg." });

        var notes = form["Notes"].ToString();

        try
        {
            task.Complete(weightKg, notes);
            
            // Force all existing logs to unchanged and new logs to added
            foreach(var entry in _context.ChangeTracker.Entries<TaskStatusLog>())
            {
                if (entry.Entity.Status == CollectionTaskStatus.Collected && entry.Entity.Id != Guid.Empty && entry.State != EntityState.Unchanged)
                    entry.State = EntityState.Added;
                else if (entry.State == EntityState.Modified)
                    entry.State = EntityState.Unchanged;
            }
            
            // Xử lý hình ảnh xác nhận (nếu có)
            var images = form.Files.GetFiles("Images");
            if (images != null && images.Count > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "tasks");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                foreach (var file in images)
                {
                    if (file.Length == 0) continue;

                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                        continue;

                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadFolder, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var imageUrl = $"/uploads/tasks/{fileName}";
                    _context.CollectionImages.Add(new CollectionImage
                    {
                        TaskId = task.Id,
                        ImageUrl = imageUrl
                    });
                }
            }

            // Đồng thời cập nhật trạng thái Report sang Collected
            // Nếu Report vẫn ở Pending, hãy Accept nó trước
            if (task.WasteReport.Status == ReportStatus.Pending)
            {
                task.WasteReport.Accept();
            }
            // Nếu Report ở Accepted, chuyển sang Assigned trước
            if (task.WasteReport.Status == ReportStatus.Accepted)
            {
                task.WasteReport.Assign();
            }
            // Bây giờ mới Collect
            task.WasteReport.Collect();

            // Thực hiện cộng điểm thưởng cho thu gom (Reward System)
            object? rewardInfo = null;
            if (task.WasteReport.WasteCategoryId.HasValue)
            {
                var rule = await _context.RewardRules
                    .FirstOrDefaultAsync(r => r.EnterpriseId == task.EnterpriseId 
                                           && r.WasteCategoryId == task.WasteReport.WasteCategoryId.Value 
                                           && r.IsActive);
                
                if (rule != null)
                {
                    int earnedPoints = rule.PointsPerReport + rule.BonusQuality;

                    var reward = new RewardPoints
                    {
                        Id = Guid.NewGuid(),
                        CitizenId = task.WasteReport.CitizenId,
                        ReportId = task.ReportId,
                        Points = earnedPoints,
                        Reason = $"Reward for collected waste report {task.ReportId}",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.RewardPoints.Add(reward);

                    // Set rewardInfo để trả về client
                    rewardInfo = new
                    {
                        rewardPointsId = reward.Id,
                        points = reward.Points,
                        reason = reward.Reason,
                        notificationMessage = $"✅ Thu gom thành công! +{reward.Points} điểm thưởng"
                    };

                    // Thông báo cho Citizen qua SignalR nếu họ đang online
                    await _hubContext.Clients.User(task.WasteReport.CitizenId.ToString())
                        .SendAsync("RewardReceived", reward.Points, reward.Reason);
                }
            }

            await _context.SaveChangesAsync();
            
            // Phát sóng sự kiện SignalR cho task status
            await _hubContext.Clients.All.SendAsync("TaskStatusUpdated", id, CollectionTaskStatus.Collected.ToString());

            var response = new
            {
                message = "Task completed successfully.",
                taskId = id,
                reward = rewardInfo
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    /// <summary>
    /// Lấy thống kê công việc của Collector (Dashboard)
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var collector = await GetCurrentCollectorAsync();
        if (collector == null)
            return Unauthorized(new { message = "Collector profile not found." });

        var tasks = await _context.CollectionTasks
            .Where(t => t.CollectorId == collector.Id)
            .ToListAsync();

        var totalAssigned = tasks.Count(t => t.Status == CollectionTaskStatus.Assigned);
        var totalOnTheWay = tasks.Count(t => t.Status == CollectionTaskStatus.OnTheWay);
        var totalCollected = tasks.Count(t => t.Status == CollectionTaskStatus.Collected);
        var totalWeight = tasks.Where(t => t.Status == CollectionTaskStatus.Collected && t.CollectedWeightKg.HasValue)
                               .Sum(t => t.CollectedWeightKg!.Value);

        return Ok(new
        {
            TotalAssigned = totalAssigned,
            TotalOnTheWay = totalOnTheWay,
            TotalCollected = totalCollected,
            TotalWeightKg = totalWeight
        });
    }
}
