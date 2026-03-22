using System;

namespace ExpenseManager.Application.DTOs
{
    public class UpdateExpenseDto
    {
        public Guid CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}
