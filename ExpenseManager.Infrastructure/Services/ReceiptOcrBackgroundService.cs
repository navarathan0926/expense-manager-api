using ExpenseManager.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExpenseManager.Infrastructure.Services;

public class ReceiptOcrBackgroundService : BackgroundService
{
    private readonly ChannelReceiptOcrQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReceiptOcrBackgroundService> _logger;

    public ReceiptOcrBackgroundService(
        ChannelReceiptOcrQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ReceiptOcrBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var receiptId = await _queue.DequeueAsync(stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var receiptService = scope.ServiceProvider.GetRequiredService<IReceiptService>();
                await receiptService.ProcessOcrAsync(receiptId);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OCR background processing failed.");
            }
        }
    }
}
