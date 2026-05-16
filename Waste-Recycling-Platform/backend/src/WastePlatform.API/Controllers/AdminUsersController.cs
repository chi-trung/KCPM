using MediatR;
using Microsoft.AspNetCore.Mvc;
using WastePlatform.Application.Admin.Users.Queries;
using WastePlatform.Application.Admin.Users.Commands;
using WastePlatform.Application.Admin.Dashboard.Queries;

namespace WastePlatform.API.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // Uncomment this line when the project has finished JWT Auth setup
    public class AdminUsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminUsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] string? role)
        {
            // Group URL parameters into the Query Model
            var query = new GetUsersQuery 
            { 
                Search = search, 
                Role = role 
            };

            // Send Query to the Application Layer for processing
            var users = await _mediator.Send(query);

            // Wrap the returned data in the standard format { data: [...] } that the Frontend expects
            return Ok(new { 
                message = "User list retrieved successfully",
                data = users 
            });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _mediator.Send(new GetDashboardStatsQuery());
            return Ok(new { data = stats });
        }

        // 1. API Add user
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var newId = await _mediator.Send(command);
            return Ok(new { message = "User created successfully", data = newId });
        }

        // 2. API Lock / Unlock
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var result = await _mediator.Send(new ToggleUserStatusCommand { UserId = id });
            if (!result) return NotFound(new { message = "User not found" });
            
            return Ok(new { message = "Status updated successfully" });
        }

        // 3. API Edit Role
        [HttpPatch("{id}/role")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateUserRoleCommand command)
        {
            // Ensure the ID in the URL and in the body match
            command.UserId = id; 
            var result = await _mediator.Send(command);
            if (!result) return NotFound(new { message = "User not found" });
            
            return Ok(new { message = "Role updated successfully" });
        }
    }
}