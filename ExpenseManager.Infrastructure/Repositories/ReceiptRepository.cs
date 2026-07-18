using ExpenseManager.Application.DTOs;
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
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
    }

    public async Task<Receipt?> GetOwnedWithExpensesAsync(Guid id, Guid userId)
    {
        return await _dbSet
            .Include(r => r.Expenses.Where(e => !e.IsDeleted))
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
    }

    public async Task<IReadOnlyList<ReceiptSummary>> ListSummariesByUserAsync(Guid userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReceiptSummary
            {
                Id = r.Id,
                FileName = r.FileName,
                FileUrl = r.FileUrl,
                ContentType = r.ContentType,
                Size = r.Size,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                LineItemCount = r.LineItemCount
            })
            .ToListAsync();
    }

    public async Task<ReceiptProcessingStatusDto?> GetProcessingStatusAsync(Guid id, Guid userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.Id == id && r.UserId == userId)
            .Select(r => new ReceiptProcessingStatusDto
            {
                ReceiptId = r.Id,
                Status = r.Status,
                LineItemCount = r.LineItemCount,
                OcrErrorMessage = r.OcrErrorMessage
            })
            .FirstOrDefaultAsync();
    }

    public async Task ClearExpenseLinksAsync(Guid receiptId)
    {
        await _context.Expenses
            .Where(e => e.ReceiptId == receiptId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.ReceiptId, (Guid?)null));
    }
}
