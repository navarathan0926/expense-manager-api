using ExpenseManager.Application.Repositories;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Repositories;

public class ReceiptRepository : Repository<Receipt>, IReceiptRepository
{
    public ReceiptRepository(ExpenseManagerDbContext context) : base(context)
    {
    }

    public async Task<Receipt?> GetOwnedAsync(Guid id, Guid userId)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
    }

    public async Task<IEnumerable<Receipt>> ListByUserAsync(Guid userId)
    {
        return await _dbSet
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task ClearExpenseLinksAsync(Guid receiptId)
    {
        var expenses = await _context.Expenses
            .Where(e => e.ReceiptId == receiptId)
            .ToListAsync();

        foreach (var expense in expenses)
            expense.ReceiptId = null;
    }
}
