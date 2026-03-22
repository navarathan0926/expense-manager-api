using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Interfaces;

public interface ICsvExportService
{
    Task<byte[]> ExportExpensesAsync(Guid userId, ExpenseFilterDto filters);
}
