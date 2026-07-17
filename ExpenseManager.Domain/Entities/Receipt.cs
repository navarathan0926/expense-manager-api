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
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
