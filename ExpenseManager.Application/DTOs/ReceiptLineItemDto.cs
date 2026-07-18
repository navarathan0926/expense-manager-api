namespace ExpenseManager.Application.DTOs;

public class ReceiptLineItemDto
{
    public string? Description { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public Guid? SuggestedCategoryId { get; set; }
}
