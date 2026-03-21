using System;

namespace ExpenseManager.Application.DTOs
{
    public class ExpenseResponseDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public decimal? ExchangeRate { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset Date { get; set; }
        public string CategoryName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
    }
}
