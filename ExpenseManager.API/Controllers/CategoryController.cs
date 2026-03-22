using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

// [Route("api/v1/categories")]
public class CategoryController : BaseController
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAll()
    {
        var userId = User.Identity?.IsAuthenticated == true ? CurrentUserId : Guid.Empty;
        var categories = await _categoryService.GetAllAsync(userId);
        return Ok(categories);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> Create([FromBody] CreateCategoryDto dto)
    {
        var created = await _categoryService.CreateAsync(dto, CurrentUserId);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _categoryService.DeleteAsync(id, CurrentUserId);
        return NoContent();
    }
}
