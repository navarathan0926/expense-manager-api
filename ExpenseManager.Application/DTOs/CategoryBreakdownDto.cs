namespace ExpenseManager.Application.DTOs;

public class CategoryBreakdownDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
}
