using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Interfaces;

public interface IReportService
{
    Task<MonthlySummaryDto> GetMonthlySummaryAsync(Guid userId, int year, int month);
    Task<IEnumerable<CategoryBreakdownDto>> GetCategoryBreakdownAsync(Guid userId, int year, int month);
}
