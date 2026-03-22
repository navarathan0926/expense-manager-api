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
        var filter = new ExpenseFilterDto
        {
            FromDate = new DateOnly(year, month, 1),
            ToDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month))
        };
        
        var expenseList = (await _expenseRepository.GetFilteredAsync(userId, filter)).ToList();

        var breakdown = expenseList
            .GroupBy(e => e.CategoryId)
            .Select(g => new CategoryBreakdownDto
            {
                CategoryId = g.Key,
                CategoryName = g.First().Category?.Name ?? "Unknown",
                TotalAmount = g.Sum(e => e.Amount)
            })
            .ToList();

        return breakdown;
    }

    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(Guid userId, int year, int month)
    {
        var filter = new ExpenseFilterDto
        {
            FromDate = new DateOnly(year, month, 1),
            ToDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month))
        };
        
        var expenseList = (await _expenseRepository.GetFilteredAsync(userId, filter)).ToList();

        var count = expenseList.Count;
        var totalAmount = expenseList.Sum(e => e.Amount);
        var average = count > 0 ? totalAmount / count : 0;

        return new MonthlySummaryDto
        {
            Year = year,
            Month = month,
            TotalAmount = totalAmount,
            TransactionCount = count,
            AverageTransactionAmount = average
        };
    }
}
