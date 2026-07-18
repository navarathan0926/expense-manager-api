using ExpenseManager.Application.DTOs;
using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Repositories;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<Expense?> GetByIdWithDetailsAsync(Guid id);
    Task<IReadOnlyList<Expense>> GetByIdsWithDetailsAsync(IReadOnlyList<Guid> ids, Guid userId);
    Task<IEnumerable<Expense>> GetFilteredAsync(Guid userId, ExpenseFilterDto filters);
    Task<MonthlySummaryDto> GetMonthlySummaryAsync(Guid userId, int year, int month);
    Task<IReadOnlyList<CategoryBreakdownDto>> GetCategoryBreakdownAsync(Guid userId, int year, int month);
}
