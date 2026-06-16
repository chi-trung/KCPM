using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WastePlatform.Application.Admin.Analytics.Queries;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "Admin")]
public class AdminAnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminAnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get analytics overview</summary>
    /// <remarks>
    /// Retrieve overview statistics including total reports, complaints, users, enterprises, and collectors.
    /// Admin only endpoint.
    /// </remarks>
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        try
        {
            var result = await _mediator.Send(new GetAnalyticsOverviewQuery());

            return Ok(new
            {
                message = "Analytics overview retrieved successfully",
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Get report analytics</summary>
    /// <remarks>
    /// Retrieve detailed report statistics including status breakdown and category distribution.
    /// Supports optional date range filtering.
    /// Admin only endpoint.
    /// </remarks>
    [HttpGet("reports")]
    public async Task<IActionResult> GetReportAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
        {
            return BadRequest(new { message = "Start date must be less than or equal to end date" });
        }

        try
        {
            var result = await _mediator.Send(new GetReportAnalyticsQuery 
            { 
                StartDate = startDate, 
                EndDate = endDate 
            });

            return Ok(new
            {
                message = "Report analytics retrieved successfully",
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Get user analytics</summary>
    /// <remarks>
    /// Retrieve detailed user statistics including breakdown by role and verification status.
    /// Admin only endpoint.
    /// </remarks>
    [HttpGet("users")]
    public async Task<IActionResult> GetUserAnalytics()
    {
        try
        {
            var result = await _mediator.Send(new GetUserAnalyticsQuery());

            return Ok(new
            {
                message = "User analytics retrieved successfully",
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Get waste analytics</summary>
    /// <remarks>
    /// Retrieve detailed waste statistics including waste by category and monthly distribution.
    /// Supports optional date range filtering.
    /// Admin only endpoint.
    /// </remarks>
    [HttpGet("waste")]
    public async Task<IActionResult> GetWasteAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
        {
            return BadRequest(new { message = "Start date must be less than or equal to end date" });
        }

        try
        {
            var result = await _mediator.Send(new GetWasteAnalyticsQuery 
            { 
                StartDate = startDate, 
                EndDate = endDate 
            });

            return Ok(new
            {
                message = "Waste analytics retrieved successfully",
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Get analytics summary</summary>
    /// <remarks>
    /// Retrieve a comprehensive summary of all analytics including overview, reports, users, and waste data.
    /// Supports optional date range filtering.
    /// Admin only endpoint.
    /// </remarks>
    [HttpGet("summary")]
    public async Task<IActionResult> GetAnalyticsSummary([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
        {
            return BadRequest(new { message = "Start date must be less than or equal to end date" });
        }
        try
        {
            var result = await _mediator.Send(new GetAnalyticsSummaryQuery 
            { 
                StartDate = startDate, 
                EndDate = endDate 
            });

            return Ok(new
            {
                message = "Analytics summary retrieved successfully",
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
