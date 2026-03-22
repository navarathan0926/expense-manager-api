using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string email, string role);
}
