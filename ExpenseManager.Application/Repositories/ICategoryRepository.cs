using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetPredefinedAndOwnedAsync(Guid userId);
    Task<IReadOnlyList<Category>> GetByIdsAsync(IReadOnlyList<Guid> ids);
}
