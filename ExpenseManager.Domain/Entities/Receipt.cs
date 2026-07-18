using ExpenseManager.Domain.Enums;

namespace ExpenseManager.Domain.Entities
{
    public class Receipt : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public required string FileName { get; set; }
        public required string BlobKey { get; set; }
        public required string FileUrl { get; set; }
        public required string ContentType { get; set; }
        public long Size { get; set; }
        public ReceiptStatus Status { get; set; } = ReceiptStatus.Pending;
        public string? Merchant { get; set; }
        public DateTimeOffset? TransactionDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Currency { get; set; }
        public decimal? TaxAmount { get; set; }
        public Guid? SuggestedCategoryId { get; set; }
        public Category? SuggestedCategory { get; set; }
        public DateTimeOffset? OcrProcessedAt { get; set; }
        public string? OcrErrorMessage { get; set; }
        public string? LineItemsJson { get; set; }
        public int LineItemCount { get; set; }
        public ReceiptImportMode? ConfirmedImportMode { get; set; }
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
