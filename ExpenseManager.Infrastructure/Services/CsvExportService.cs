using System.Globalization;
using CsvHelper;
using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Interfaces;
using ExpenseManager.Application.Repositories;

namespace ExpenseManager.Infrastructure.Services;

public class CsvExportService : ICsvExportService
{
    private readonly IExpenseRepository _expenseRepository;

    public CsvExportService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public async Task<byte[]> ExportExpensesAsync(Guid userId, ExpenseFilterDto filters)
    {
        var expenses = await _expenseRepository.GetFilteredAsync(userId, filters);

        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream);
        using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        // Map domain entities to anonymous objects or a specific DTO for CSV
        var records = expenses.Select(e => new
        {
            Date = e.Date.ToString("yyyy-MM-dd"),
            Amount = e.Amount,
            Currency = e.Currency,
            Category = e.Category?.Name ?? "Unknown",
            Description = e.Description
        });

        await csvWriter.WriteRecordsAsync(records);
        await streamWriter.FlushAsync();
        
        return memoryStream.ToArray();
    }
}
