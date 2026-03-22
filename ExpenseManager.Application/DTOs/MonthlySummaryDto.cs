namespace ExpenseManager.Application.DTOs;

public class MonthlySummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageTransactionAmount { get; set; }
}
