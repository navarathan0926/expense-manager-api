using ExpenseManager.Domain.Enums;

namespace ExpenseManager.Application.DTOs;

public class ReceiptProcessingStatusDto
{
    public Guid ReceiptId { get; set; }
    public ReceiptStatus Status { get; set; }
    public int LineItemCount { get; set; }
    public string? OcrErrorMessage { get; set; }
}
