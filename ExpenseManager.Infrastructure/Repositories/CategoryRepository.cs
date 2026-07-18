using ExpenseManager.Application.Repositories;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ExpenseManagerDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetPredefinedAndOwnedAsync(Guid userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.IsPredefined || c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Category>> GetByIdsAsync(IReadOnlyList<Guid> ids)
    {
        if (ids.Count == 0)
            return [];

        return await _dbSet
            .AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .ToListAsync();
    }
}
