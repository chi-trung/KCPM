using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WastePlatform.Application.Admin.Enterprises.Commands;
using WastePlatform.Application.Admin.Enterprises.Queries;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/admin/enterprises")]
[Authorize(Roles = "Admin")]
public class AdminEnterpriseController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminEnterpriseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get all enterprises with pagination and filtering</summary>
    /// <remarks>
    /// Retrieve a paginated list of enterprises with optional verification status filtering and search.
    /// Admin only endpoint.
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> GetEnterprises([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? isVerified = null, [FromQuery] string? searchTerm = null)
    {
        try
        {
            var result = await _mediator.Send(new GetEnterprisesQuery 
            { 
                Page = page, 
                PageSize = pageSize, 
                IsVerified = isVerified,
                SearchTerm = searchTerm
            });

            return Ok(new
            {
                message = "Enterprises retrieved successfully",
                data = result.Enterprises,
                pagination = new { total = result.Total, totalPages = result.TotalPages, page, pageSize }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Get enterprise detail by ID</summary>
    /// <remarks>
    /// Retrieve detailed information about a specific enterprise including collectors and waste types.
    /// Admin only endpoint.
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEnterpriseDetail(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetEnterpriseDetailQuery { EnterpriseId = id });

            if (result == null)
                return NotFound(new { message = "Enterprise not found" });

            return Ok(new
            {
                message = "Enterprise retrieved successfully",
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Verify an enterprise</summary>
    /// <remarks>
    /// Mark an enterprise as verified, allowing it to operate in the system.
    /// Admin only endpoint.
    /// </remarks>
    [HttpPost("{id}/verify")]
    public async Task<IActionResult> VerifyEnterprise(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new VerifyEnterpriseCommand { EnterpriseId = id });

            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                data = new { enterpriseId = result.EnterpriseId }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Reject an enterprise</summary>
    /// <remarks>
    /// Reject an enterprise application with a reason for rejection.
    /// Admin only endpoint.
    /// </remarks>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectEnterprise(Guid id, [FromBody] RejectEnterpriseRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ReasonForRejection))
                return BadRequest(new { message = "Reason for rejection is required" });

            var result = await _mediator.Send(new RejectEnterpriseCommand 
            { 
                EnterpriseId = id, 
                ReasonForRejection = request.ReasonForRejection 
            });

            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                data = new { enterpriseId = result.EnterpriseId }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}

public class RejectEnterpriseRequest
{
    public string ReasonForRejection { get; set; } = null!;
}
