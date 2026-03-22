using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Interfaces;

public interface IUserService
{
    // Admin only
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    // Both Admin and User (own profile only)
    Task<UserResponseDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin);
}
