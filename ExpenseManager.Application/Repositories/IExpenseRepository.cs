using ExpenseManager.Application.DTOs;
using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Repositories;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<Expense?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Expense>> GetFilteredAsync(Guid userId, ExpenseFilterDto filters);
}
