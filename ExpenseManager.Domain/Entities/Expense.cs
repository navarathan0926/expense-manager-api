namespace ExpenseManager.Domain.Entities
{
    public class Expense : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
        public decimal? ExchangeRate { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset Date { get; set; }
        public Guid? ReceiptId { get; set; }
        public Receipt? Receipt { get; set; }
    }
}
