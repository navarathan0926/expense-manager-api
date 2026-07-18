using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Exceptions;
using ExpenseManager.Application.Interfaces;
using ExpenseManager.Application.Repositories;
using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IReceiptRepository _receiptRepository;
    private readonly IMapper _mapper;

    public ExpenseService(
        IExpenseRepository expenseRepository,
        ICategoryRepository categoryRepository,
        IReceiptRepository receiptRepository,
        IMapper mapper)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _receiptRepository = receiptRepository;
        _mapper = mapper;
    }

    public async Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto, Guid userId)
    {
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null)
            throw new NotFoundException(nameof(Category), dto.CategoryId);

        if (!category.IsPredefined && category.UserId != userId)
            throw new UnauthorizedException("You do not have permission to use this category.");

        await EnsureReceiptOwnedAsync(dto.ReceiptId, userId);

        var expense = _mapper.Map<Expense>(dto);
        expense.UserId = userId; // Never trust from DTO, enforce from claims
        
        await _expenseRepository.AddAsync(expense);
        await _expenseRepository.SaveChangesAsync();

        var createdExpense = await _expenseRepository.GetByIdWithDetailsAsync(expense.Id);
        return _mapper.Map<ExpenseResponseDto>(createdExpense);
    }

    public async Task<IEnumerable<ExpenseResponseDto>> CreateBatchAsync(
        IReadOnlyList<CreateExpenseDto> dtos,
        Guid userId)
    {
        if (dtos.Count == 0)
            throw new BadRequestException("At least one expense is required.");

        var categoryIds = dtos.Select(d => d.CategoryId).Distinct().ToList();
        var categories = await _categoryRepository.GetByIdsAsync(categoryIds);
        if (categories.Count != categoryIds.Count)
        {
            var missingId = categoryIds.First(id => categories.All(c => c.Id != id));
            throw new NotFoundException(nameof(Category), missingId);
        }

        foreach (var category in categories)
        {
            if (!category.IsPredefined && category.UserId != userId)
                throw new UnauthorizedException("You do not have permission to use this category.");
        }

        var receiptIds = dtos
            .Select(d => d.ReceiptId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (receiptIds.Count > 0)
        {
            foreach (var receiptId in receiptIds)
                await EnsureReceiptOwnedAsync(receiptId, userId);
        }

        var expenses = new List<Expense>(dtos.Count);
        foreach (var dto in dtos)
        {
            var expense = _mapper.Map<Expense>(dto);
            expense.UserId = userId;
            expenses.Add(expense);
        }

        await _expenseRepository.AddRangeAsync(expenses);
        await _expenseRepository.SaveChangesAsync();

        var expenseIds = expenses.Select(e => e.Id).ToList();
        var loaded = await _expenseRepository.GetByIdsWithDetailsAsync(expenseIds, userId);
        return _mapper.Map<IEnumerable<ExpenseResponseDto>>(loaded);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            throw new NotFoundException(nameof(Expense), id);
        }

        if (expense.UserId != userId)
        {
            throw new UnauthorizedException("You do not have permission to delete this expense.");
        }

        _expenseRepository.Delete(expense);
        await _expenseRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<ExpenseResponseDto>> GetAllAsync(Guid userId, ExpenseFilterDto filters)
    {
        var expenses = await _expenseRepository.GetFilteredAsync(userId, filters);
        return _mapper.Map<IEnumerable<ExpenseResponseDto>>(expenses);
    }

    public async Task<ExpenseResponseDto> GetByIdAsync(Guid id, Guid userId)
    {
        var expense = await _expenseRepository.GetByIdWithDetailsAsync(id);
        if (expense == null)
        {
            throw new NotFoundException(nameof(Expense), id);
        }

        if (expense.UserId != userId)
        {
            throw new UnauthorizedException("You do not have permission to view this expense.");
        }

        return _mapper.Map<ExpenseResponseDto>(expense);
    }

    public async Task<ExpenseResponseDto> UpdateAsync(Guid id, UpdateExpenseDto dto, Guid userId)
    {
        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            throw new NotFoundException(nameof(Expense), id);
        }

        if (expense.UserId != userId)
        {
            throw new UnauthorizedException("You do not have permission to update this expense.");
        }

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null)
            throw new NotFoundException(nameof(Category), dto.CategoryId);

        if (!category.IsPredefined && category.UserId != userId)
            throw new UnauthorizedException("You do not have permission to use this category.");

        await EnsureReceiptOwnedAsync(dto.ReceiptId, userId);

        _mapper.Map(dto, expense);
        
        _expenseRepository.Update(expense);
        await _expenseRepository.SaveChangesAsync();

        var updatedExpense = await _expenseRepository.GetByIdWithDetailsAsync(expense.Id);
        return _mapper.Map<ExpenseResponseDto>(updatedExpense);
    }

    private async Task EnsureReceiptOwnedAsync(Guid? receiptId, Guid userId)
    {
        if (!receiptId.HasValue)
            return;

        var receipt = await _receiptRepository.GetOwnedAsync(receiptId.Value, userId);
        if (receipt == null)
            throw new NotFoundException(nameof(Receipt), receiptId.Value);
    }
}

