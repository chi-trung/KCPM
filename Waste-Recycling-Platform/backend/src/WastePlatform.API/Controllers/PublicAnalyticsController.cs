using MediatR;
using Microsoft.AspNetCore.Mvc;
using WastePlatform.Application.Public.Analytics.Queries;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/public/analytics")]
public class PublicAnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicAnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get public report analytics</summary>
    /// <remarks>
    /// Retrieve public waste statistics including waste by area and type for public viewing.
    /// Limited to last 3 months of data by default.
    /// Public endpoint - no authentication required.
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
            var result = await _mediator.Send(new GetPublicReportAnalyticsQuery 
            { 
                StartDate = startDate, 
                EndDate = endDate 
            });

            return Ok(new
            {
                message = "Public report analytics retrieved successfully",
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
