using ExpenseManager.Domain.Enums;

namespace ExpenseManager.Application.DTOs;

public class ConfirmReceiptExtractionDto
{
    public required string Currency { get; set; }
    public DateTimeOffset Date { get; set; }
    public ReceiptImportMode ImportMode { get; set; } = ReceiptImportMode.Itemized;
    public List<ConfirmReceiptExpenseItemDto> Expenses { get; set; } = [];
}
