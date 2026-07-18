using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Repositories;

public interface IReceiptRepository : IRepository<Receipt>
{
    Task<Receipt?> GetOwnedAsync(Guid id, Guid userId);
    Task<Receipt?> GetOwnedWithExpensesAsync(Guid id, Guid userId);
    Task<IEnumerable<Receipt>> ListByUserAsync(Guid userId);
    Task ClearExpenseLinksAsync(Guid receiptId);
}
