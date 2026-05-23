using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WastePlatform.Application.WasteCategories.Queries;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/waste-categories")]
public class WasteCategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public WasteCategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Lấy danh sách tất cả các loại rác</summary>
    /// <remarks>
    /// Endpoint này trả về danh sách các loại rác có sẵn trong hệ thống
    /// Không yêu cầu xác thực - endpoint công khai
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        try
        {
            var data = await _mediator.Send(new GetAllCategoriesQuery());

            return Ok(new
            {
                message = "Categories retrieved successfully",
                data = data
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>Lấy chi tiết loại rác theo ID</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        try
        {
            var data = await _mediator.Send(new GetCategoryByIdQuery { Id = id });

            if (data == null)
                return NotFound(new { message = "Category not found" });

            return Ok(new
            {
                message = "Category retrieved successfully",
                data = data
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
