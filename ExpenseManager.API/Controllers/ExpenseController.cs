using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

[Authorize]
// [Route("api/v1/expenses")]
public class ExpenseController : BaseController
{
    private readonly IExpenseService _expenseService;

    public ExpenseController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseResponseDto>>> GetAll([FromQuery] ExpenseFilterDto filters)
    {
        var expenses = await _expenseService.GetAllAsync(CurrentUserId, filters);
        return Ok(expenses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseResponseDto>> GetById(Guid id)
    {
        var expense = await _expenseService.GetByIdAsync(id, CurrentUserId);
        return Ok(expense);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponseDto>> Create([FromBody] CreateExpenseDto dto)
    {
        var created = await _expenseService.CreateAsync(dto, CurrentUserId);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExpenseResponseDto>> Update(Guid id, [FromBody] UpdateExpenseDto dto)
    {
        var updated = await _expenseService.UpdateAsync(id, dto, CurrentUserId);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _expenseService.DeleteAsync(id, CurrentUserId);
        return NoContent();
    }
}
