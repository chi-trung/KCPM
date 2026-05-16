using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WastePlatform.Application.Admin.Complaints.Commands;
using WastePlatform.Application.Admin.Complaints.DTOs;
using WastePlatform.Application.Admin.Complaints.Queries;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Infrastructure.SignalR;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/admin/complaints")]
[Authorize(Roles = "Admin")]
public class AdminComplaintsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<TaskHub> _hubContext;
    private readonly INotificationService _notificationService;

    public AdminComplaintsController(IMediator mediator, IHubContext<TaskHub> hubContext, INotificationService notificationService)
    {
        _mediator = mediator;
        _hubContext = hubContext;
        _notificationService = notificationService;
    }

    /// <summary>Get all complaints with pagination and filtering</summary>
    /// <remarks>
    /// Retrieve a paginated list of complaints with optional status filtering and search.
    /// Admin only endpoint.
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> GetComplaints([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null, [FromQuery] string? searchTerm = null)
    {
        try
        {
            var result = await _mediator.Send(new GetComplaintsQuery 
            { 
                Page = page, 
                PageSize = pageSize, 
                Status = status,
                SearchTerm = searchTerm
            });

            return Ok(new
            {
                message = "Complaints retrieved successfully",
                data = result.Complaints,
                pagination = new { total = result.Total, totalPages = result.TotalPages, page, pageSize }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Get complaint detail by ID</summary>
    /// <remarks>
    /// Retrieve detailed information about a specific complaint including citizen details and related report.
    /// Admin only endpoint.
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetComplaintDetail(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetComplaintDetailQuery { ComplaintId = id });

            if (result == null)
                return NotFound(new { message = "Complaint not found" });

            return Ok(new
            {
                message = "Complaint retrieved successfully",
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Resolve a complaint</summary>
    /// <remarks>
    /// Mark a complaint as resolved with an admin response.
    /// Admin only endpoint.
    /// </remarks>
    [HttpPost("{id}/resolve")]
    public async Task<IActionResult> ResolveComplaint(Guid id, [FromBody] ComplaintResponseRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.AdminResponse))
                return BadRequest(new { message = "Admin response is required" });

            var result = await _mediator.Send(new ResolveComplaintCommand
            {
                ComplaintId = id,
                AdminResponse = request.AdminResponse
            });

            if (!result.Success)
                return NotFound(new { message = result.Message });

            // Send SignalR notification and create in-app notification
            try
            {
                // retrieve complaint to know reporter id
                var complaint = await _mediator.Send(new WastePlatform.Application.Complaints.Queries.GetComplaintByIdQuery { Id = id });
                if (complaint != null)
                {
                    await _hubContext.Clients.User(complaint.CitizenId.ToString()).SendAsync("ComplaintResolved", new
                    {
                        complaintId = id,
                        message = "Your complaint has been resolved",
                        adminResponse = request.AdminResponse
                    });

                    // Gửi thông báo: Phản hồi được trả lời (Trigger #7)
                    await _notificationService.NotifyComplaintRepliedAsync(
                        complaint.CitizenId,
                        id,
                        "Quản trị viên");
                }
            }
            catch
            {
                // best-effort
            }

            return Ok(new
            {
                message = result.Message,
                data = new { complaintId = result.ComplaintId }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Reject a complaint</summary>
    /// <remarks>
    /// Mark a complaint as rejected with an admin response explaining the reason.
    /// Admin only endpoint.
    /// </remarks>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectComplaint(Guid id, [FromBody] ComplaintResponseRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.AdminResponse))
                return BadRequest(new { message = "Admin response is required" });

            var result = await _mediator.Send(new RejectComplaintCommand 
            { 
                ComplaintId = id, 
                AdminResponse = request.AdminResponse 
            });

            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                data = new { complaintId = result.ComplaintId }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}

public class ComplaintResponseRequest
{
    public string AdminResponse { get; set; } = null!;
}
