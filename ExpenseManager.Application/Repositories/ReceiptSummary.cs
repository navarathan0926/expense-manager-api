using ExpenseManager.Domain.Enums;

namespace ExpenseManager.Application.Repositories;

public class ReceiptSummary
{
    public Guid Id { get; init; }
    public required string FileName { get; init; }
    public required string FileUrl { get; init; }
    public required string ContentType { get; init; }
    public long Size { get; init; }
    public ReceiptStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public int LineItemCount { get; init; }
}
