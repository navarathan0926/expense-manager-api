using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Repositories;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Repositories;

public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    public ExpenseRepository(ExpenseManagerDbContext context) : base(context)
    {
    }

    public async Task<Expense?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(e => e.Category)
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Expense>> GetFilteredAsync(Guid userId, ExpenseFilterDto filters)
    {
        var query = _dbSet
                    .Where(e => e.UserId == userId)
                    .Include(e => e.Category)
                    .Include(e => e.User)
                    .AsQueryable();

        if (filters.FromDate.HasValue)
        {
            query = query.Where(e =>
                e.Date >= filters.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        if (filters.ToDate.HasValue)
        {
            query = query.Where(e =>
                e.Date <= filters.ToDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        }

        if (filters.CategoryId.HasValue)
        {
            query = query.Where(e => e.CategoryId == filters.CategoryId.Value);
        }

        if (filters.MinAmount.HasValue)
        {
            query = query.Where(e => e.Amount >= filters.MinAmount.Value);
        }

        if (filters.MaxAmount.HasValue)
        {
            query = query.Where(e => e.Amount <= filters.MaxAmount.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Currency))
        {
            query = query.Where(e => e.Currency == filters.Currency);
        }

        return await query.ToListAsync();
    }
}
