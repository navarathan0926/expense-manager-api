using ExpenseManager.Domain.Enums;

namespace ExpenseManager.Application.DTOs;

public class ReceiptDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
    public ReceiptStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
