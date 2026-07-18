namespace ExpenseManager.Application.DTOs;

public class ConfirmReceiptExpenseItemDto
{
    public decimal Amount { get; set; }
    public Guid CategoryId { get; set; }
    public string? Description { get; set; }
}
