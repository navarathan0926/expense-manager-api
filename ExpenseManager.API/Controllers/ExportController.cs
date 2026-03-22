using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

[Authorize]
// [Route("api/v1/export")]
public class ExportController : BaseController
{
    private readonly ICsvExportService _csvExportService;

    public ExportController(ICsvExportService csvExportService)
    {
        _csvExportService = csvExportService;
    }

    [HttpGet("csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] ExpenseFilterDto filters)
    {
        var bytes = await _csvExportService.ExportExpensesAsync(CurrentUserId, filters);
        if (bytes == null)
            return NoContent();
        var fileName = $"expenses_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }
}
