using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WastePlatform.Application.Complaints.Commands;
using WastePlatform.Application.Complaints.Queries;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Application.Reports.Queries;
using WastePlatform.Application.Rewards.Queries;
using WastePlatform.Application.Citizens.Profile.Commands;
using WastePlatform.Application.Citizens.Profile.Queries;
using WastePlatform.Application.Citizens.Profile.DTOs;
using WastePlatform.Domain.Enums;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/citizens")]
[Authorize(Roles = "Citizen")]
public class CitizenController : ControllerBase
{
    private readonly IMediator _mediator;

    public CitizenController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get total reward points for current citizen</summary>
    [HttpGet("rewards")]
    public async Task<IActionResult> GetTotalRewards()
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { message = "Invalid or missing user ID in token" });

            var result = await _mediator.Send(new GetTotalPointsQuery { CitizenId = userId });
            return Ok(new { message = "Total rewards retrieved successfully", data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Get reward points history with pagination</summary>
    [HttpGet("rewards/history")]
    public async Task<IActionResult> GetRewardHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { message = "Invalid or missing user ID in token" });

            if (page < 1 || pageSize < 1)
                return BadRequest(new { message = "Page and PageSize must be greater than 0" });

            var result = await _mediator.Send(new GetRewardHistoryQuery
            {
                CitizenId = userId,
                Page = page,
                PageSize = pageSize
            });

            return Ok(new
            {
                message = "Reward history retrieved successfully",
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Get leaderboard of citizens by points</summary>
    [HttpGet("rewards/leaderboard")]
    [AllowAnonymous] // <--- DÒNG NÀY GIÚP API PUBLIC, KHÔNG CẦN LOGIN VẪN XEM ĐƯỢC
    public async Task<IActionResult> GetLeaderboard([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1 || pageSize < 1)
                return BadRequest(new { message = "Page and PageSize must be greater than 0" });

            var result = await _mediator.Send(new GetLeaderboardQuery
            {
                Page = page,
                PageSize = pageSize
            });

            return Ok(new
            {
                message = "Leaderboard retrieved successfully",
                // Trả về trực tiếp mảng Leaderboard vào biến data để khớp với React Frontend nãy tui viết
                data = result.Leaderboard, 
                pagination = new {
                    page = result.Page,
                    pageSize = result.PageSize,
                    total = result.Total,
                    totalPages = result.TotalPages
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Get leaderboard by areas/districts</summary>
    [HttpGet("rewards/leaderboard/area")]
    [AllowAnonymous] // Tiếp tục mở cửa tự do cho API này
    public async Task<IActionResult> GetAreaLeaderboard([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1 || pageSize < 1)
                return BadRequest(new { message = "Page and PageSize must be greater than 0" });

            var result = await _mediator.Send(new GetAreaLeaderboardQuery
            {
                Page = page,
                PageSize = pageSize
            });

            return Ok(new
            {
                message = "Area leaderboard retrieved successfully",
                data = result.Leaderboard, 
                pagination = new {
                    page = result.Page,
                    pageSize = result.PageSize,
                    total = result.Total,
                    totalPages = result.TotalPages
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Get current citizen profile</summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { message = "Invalid or missing user ID in token" });

            var result = await _mediator.Send(new GetProfileQuery { UserId = userId });
            return Ok(new { message = "Profile retrieved successfully", data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Update current citizen profile</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto profile)
    {
        try
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { message = "Invalid or missing user ID in token" });

            if (string.IsNullOrWhiteSpace(profile.FullName))
                return BadRequest(new { message = "Full name is required" });

            var result = await _mediator.Send(new UpdateProfileCommand 
            { 
                UserId = userId, 
                Profile = profile 
            });
            
            return Ok(new { message = "Profile updated successfully", data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
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