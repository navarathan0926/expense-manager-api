using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllUsersAsync();
}
