using AutoMapper;
using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Exceptions;
using ExpenseManager.Application.Interfaces;
using ExpenseManager.Application.Repositories;
using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto, Guid userId)
    {
        var existingCategories = await _categoryRepository.GetPredefinedAndOwnedAsync(userId);
        if (existingCategories.Any(c => c.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException($"Category with name '{dto.Name}' already exists.");
        }

        var category = _mapper.Map<Category>(dto);
        category.IsPredefined = false;
        category.UserId = userId; // Never trust userId from DTO

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return _mapper.Map<CategoryResponseDto>(category);
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync(Guid userId)
    {
        var categories = await _categoryRepository.GetPredefinedAndOwnedAsync(userId);
        return _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            throw new NotFoundException(nameof(Category), id);
        }

        if (category.IsPredefined)
        {
            throw new UnauthorizedException("Predefined categories cannot be deleted.");
        }

        if (category.UserId != userId)
        {
            throw new UnauthorizedException("You are not authorized to delete this category.");
        }

        _categoryRepository.Delete(category);
        await _categoryRepository.SaveChangesAsync();
    }
}
