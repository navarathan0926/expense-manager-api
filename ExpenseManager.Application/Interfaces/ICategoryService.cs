using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponseDto>> GetAllAsync(Guid userId);
    Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}
