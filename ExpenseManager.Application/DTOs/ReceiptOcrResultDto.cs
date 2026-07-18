namespace ExpenseManager.Application.DTOs;

public class ReceiptOcrResultDto
{
    public string? Merchant { get; set; }
    public DateTimeOffset? TransactionDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Currency { get; set; }
    public decimal? TaxAmount { get; set; }
    public List<ReceiptLineItemDto>? Items { get; set; }
}
