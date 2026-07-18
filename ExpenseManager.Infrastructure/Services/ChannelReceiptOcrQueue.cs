using System.Threading.Channels;
using ExpenseManager.Application.Interfaces;

namespace ExpenseManager.Infrastructure.Services;

public class ChannelReceiptOcrQueue : IReceiptOcrQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

    public ValueTask EnqueueAsync(Guid receiptId, CancellationToken ct = default)
    {
        return _channel.Writer.WriteAsync(receiptId, ct);
    }

    public ValueTask<Guid> DequeueAsync(CancellationToken ct = default)
    {
        return _channel.Reader.ReadAsync(ct);
    }
}
