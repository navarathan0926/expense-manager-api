using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Interfaces;

public interface IExpenseService
{
    Task<IEnumerable<ExpenseResponseDto>> GetAllAsync(Guid userId, ExpenseFilterDto filters);
    Task<ExpenseResponseDto> GetByIdAsync(Guid id, Guid userId);
    Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto, Guid userId);
    Task<ExpenseResponseDto> UpdateAsync(Guid id, UpdateExpenseDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}
