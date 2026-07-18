using ExpenseManager.Domain.Enums;

namespace ExpenseManager.Application.DTOs;

public class ReceiptExtractionDto
{
    public Guid ReceiptId { get; set; }
    public ReceiptStatus Status { get; set; }
    public string? Merchant { get; set; }
    public DateTimeOffset? TransactionDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Currency { get; set; }
    public decimal? TaxAmount { get; set; }
    public Guid? SuggestedCategoryId { get; set; }
    public string? OcrErrorMessage { get; set; }
    public List<ReceiptLineItemDto> LineItems { get; set; } = [];
}
