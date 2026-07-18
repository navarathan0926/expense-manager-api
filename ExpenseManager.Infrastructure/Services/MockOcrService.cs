using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Interfaces;

namespace ExpenseManager.Infrastructure.Services;

public class MockOcrService : IOcrService
{
    public Task<ReceiptOcrResultDto> AnalyzeReceiptAsync(Stream content, string contentType, CancellationToken ct = default)
    {
        return Task.FromResult(new ReceiptOcrResultDto
        {
            Merchant = "Sample Coffee Shop",
            TransactionDate = DateTimeOffset.UtcNow.AddDays(-1),
            TotalAmount = 12.50m,
            Currency = "USD",
            TaxAmount = 1.05m,
            Items =
            [
                new ReceiptLineItemDto
                {
                    Description = "Latte",
                    Quantity = 1,
                    UnitPrice = 4.50m,
                    TotalPrice = 4.50m
                },
                new ReceiptLineItemDto
                {
                    Description = "Sandwich",
                    Quantity = 1,
                    UnitPrice = 8.00m,
                    TotalPrice = 8.00m
                }
            ]
        });
    }
}
