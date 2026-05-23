using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WastePlatform.Application.Enterprise.Analytics.Queries;
using WastePlatform.Application.Enterprise.Queries;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/enterprise/analytics")]
[Authorize(Roles = "Enterprise")]
public class EnterpriseAnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EnterpriseAnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get enterprise report analytics</summary>
    /// <remarks>
    /// Retrieve report statistics for enterprise's scope including waste by area and type.
    /// Supports optional date range filtering.
    /// Enterprise only endpoint.
    /// </remarks>
    [HttpGet("reports")]
    public async Task<IActionResult> GetReportAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            var enterprise = await _mediator.Send(new GetEnterpriseByUserIdQuery { UserId = userId });
            if (enterprise == null)
            {
                return Unauthorized(new { message = "Enterprise profile not found for current user." });
            }

            var result = await _mediator.Send(new GetEnterpriseReportAnalyticsQuery 
            { 
                EnterpriseId = enterprise.Id,
                StartDate = startDate, 
                EndDate = endDate 
            });

            return Ok(new
            {
                message = "Enterprise report analytics retrieved successfully",
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
