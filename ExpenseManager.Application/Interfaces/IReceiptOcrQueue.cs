namespace ExpenseManager.Application.Interfaces;

public interface IReceiptOcrQueue
{
    ValueTask EnqueueAsync(Guid receiptId, CancellationToken ct = default);
}
