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
            .AsNoTracking()
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IReadOnlyList<Expense>> GetByIdsWithDetailsAsync(IReadOnlyList<Guid> ids, Guid userId)
    {
        if (ids.Count == 0)
            return [];

        return await _dbSet
            .AsNoTracking()
            .Where(e => ids.Contains(e.Id) && e.UserId == userId)
            .Include(e => e.Category)
            .OrderBy(e => e.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetFilteredAsync(Guid userId, ExpenseFilterDto filters)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Include(e => e.Category)
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

        return await query
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(Guid userId, int year, int month)
    {
        var from = new DateTimeOffset(new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc));
        var to = new DateTimeOffset(new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59, DateTimeKind.Utc));

        var summary = await _dbSet
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= from && e.Date <= to)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Count = g.Count(),
                Total = g.Sum(e => e.Amount)
            })
            .FirstOrDefaultAsync();

        var count = summary?.Count ?? 0;
        var total = summary?.Total ?? 0m;

        return new MonthlySummaryDto
        {
            Year = year,
            Month = month,
            TotalAmount = total,
            TransactionCount = count,
            AverageTransactionAmount = count > 0 ? total / count : 0
        };
    }

    public async Task<IReadOnlyList<CategoryBreakdownDto>> GetCategoryBreakdownAsync(Guid userId, int year, int month)
    {
        var from = new DateTimeOffset(new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc));
        var to = new DateTimeOffset(new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59, DateTimeKind.Utc));

        return await _dbSet
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= from && e.Date <= to)
            .GroupBy(e => new { e.CategoryId, e.Category.Name })
            .Select(g => new CategoryBreakdownDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                TotalAmount = g.Sum(e => e.Amount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToListAsync();
    }
}
