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
        return await _dbSet.Where(c => c.IsPredefined || c.UserId == userId).ToListAsync();
    }
}
