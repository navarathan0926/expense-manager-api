using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Interfaces;

public interface IOcrService
{
    Task<ReceiptOcrResultDto> AnalyzeReceiptAsync(Stream content, string contentType, CancellationToken ct = default);
}
