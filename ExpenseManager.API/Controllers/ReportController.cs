using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

[Authorize]
// [Route("api/v1/reports")]
public class ReportController : BaseController
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("monthly-summary")]
    public async Task<ActionResult<MonthlySummaryDto>> GetMonthlySummary(
        [FromQuery] ReportQueryDto query)
    {
        var summary = await _reportService.GetMonthlySummaryAsync(CurrentUserId, query.Year, query.Month);
        return Ok(summary);
    }

    [HttpGet("category-breakdown")]
    public async Task<ActionResult<IEnumerable<CategoryBreakdownDto>>> GetCategoryBreakdown(
        [FromQuery] ReportQueryDto query)
    {
        var breakdown = await _reportService.GetCategoryBreakdownAsync(
            CurrentUserId, query.Year, query.Month);
        return Ok(breakdown);
    }
}
