using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Interfaces;
using ExpenseManager.Application.Repositories;

namespace ExpenseManager.Application.Services;

public class ReportService : IReportService
{
    private readonly IExpenseRepository _expenseRepository;

    public ReportService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public async Task<IEnumerable<CategoryBreakdownDto>> GetCategoryBreakdownAsync(Guid userId, int year, int month)
    {
        var breakdown = await _expenseRepository.GetCategoryBreakdownAsync(userId, year, month);
        return breakdown;
    }

    public Task<MonthlySummaryDto> GetMonthlySummaryAsync(Guid userId, int year, int month)
    {
        return _expenseRepository.GetMonthlySummaryAsync(userId, year, month);
    }
}
