namespace ExpenseManager.Application.DTOs;

public class ExpenseFilterDto
{
    public DateTimeOffset? FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Currency { get; set; }
}
