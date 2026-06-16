using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WastePlatform.Application.Complaints.Commands;
using WastePlatform.Application.Complaints.Queries;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Domain.Enums;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/complaints")]
[Authorize(Roles = "Citizen")]
public class ComplaintsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ComplaintsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Create a new complaint</summary>
    [HttpPost]
    public async Task<IActionResult> CreateComplaint([FromBody] CreateComplaintDto dto)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { message = "Invalid or missing user ID in token" });

            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { message = "Complaint content cannot be empty" });

            var complaintId = await _mediator.Send(new CreateComplaintCommand
            {
                CitizenId = userId,
                Content = dto.Content,
                ReportId = dto.ReportId,
                EnterpriseId = dto.EnterpriseId
            });

            var complaint = await _mediator.Send(new GetComplaintByIdQuery { Id = complaintId });

            return CreatedAtAction(nameof(GetComplaintDetail), new { id = complaintId }, new
            {
                message = "Complaint created successfully",
                data = complaint
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Get citizen's complaints with pagination</summary>
    [HttpGet]
    public async Task<IActionResult> GetComplaints([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] ComplaintStatus? status = null)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { message = "Invalid or missing user ID in token" });

            if (page < 1 || pageSize < 1)
                return BadRequest(new { message = "Page and PageSize must be greater than 0" });

            var result = await _mediator.Send(new GetCitizenComplaintsQuery
            {
                CitizenId = userId,
                Page = page,
                PageSize = pageSize,
                Status = status
            });

            return Ok(new
            {
                message = "Complaints retrieved successfully",
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Get complaint detail by ID</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetComplaintDetail([Required] Guid id)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { message = "Invalid or missing user ID in token" });

            var complaint = await _mediator.Send(new GetComplaintByIdQuery { Id = id });

            if (complaint == null)
                return NotFound(new { message = "Complaint not found" });

            // Check authorization - citizen can only view their own complaints
            if (complaint.CitizenId != userId)
                return Forbid();

            return Ok(new
            {
                message = "Complaint retrieved successfully",
                data = complaint
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/escalate")]
    [Authorize(Roles = "Citizen")]
    public async Task<IActionResult> EscalateToAdmin([Required] Guid id, [FromBody] CitizenEscalateRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { message = "Invalid or missing user ID in token" });

            var result = await _mediator.Send(new CitizenEscalateComplaintCommand
            {
                ComplaintId = id,
                CitizenId = userId,
                Reason = request.Reason ?? string.Empty
            });

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                data = result
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] EscalateToAdmin failed: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
                Console.WriteLine($"[ERROR] Inner: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message, type = ex.GetType().Name });
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Guid.Empty;
        return userId;
    }
}

public class CitizenEscalateRequest
{
    public string? Reason { get; set; }
}
