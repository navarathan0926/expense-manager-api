using ExpenseManager.Application.DTOs;

namespace ExpenseManager.Application.Interfaces;

public interface IReceiptService
{
    Task<ReceiptDto> UploadAsync(Stream content, string fileName, string contentType, long size, Guid userId);
    Task<ReceiptDto> GetAsync(Guid id, Guid userId);
    Task<IEnumerable<ReceiptDto>> ListByUserAsync(Guid userId);
    Task<ReceiptFileDto> GetFileAsync(Guid id, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task ProcessOcrAsync(Guid receiptId);
    Task<ReceiptExtractionDto> GetExtractionAsync(Guid id, Guid userId);
    Task<ReceiptProcessingStatusDto> GetProcessingStatusAsync(Guid id, Guid userId);
    Task<IEnumerable<ExpenseResponseDto>> ConfirmExtractionAsync(Guid id, ConfirmReceiptExtractionDto dto, Guid userId);
    Task RetryOcrAsync(Guid id, Guid userId);
}
