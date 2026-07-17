using AutoMapper;
using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Exceptions;
using ExpenseManager.Application.Interfaces;
using ExpenseManager.Application.Repositories;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Enums;

namespace ExpenseManager.Application.Services;

public class ReceiptService : IReceiptService
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/pdf"
    };

    private const long MaxSizeBytes = 5 * 1024 * 1024;

    private readonly IReceiptRepository _receiptRepository;
    private readonly IBlobStore _blobStore;
    private readonly IMapper _mapper;

    public ReceiptService(
        IReceiptRepository receiptRepository,
        IBlobStore blobStore,
        IMapper mapper)
    {
        _receiptRepository = receiptRepository;
        _blobStore = blobStore;
        _mapper = mapper;
    }

    public async Task<ReceiptDto> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        long size,
        Guid userId)
    {
        ValidateFile(fileName, contentType, size);

        var blobKey = $"{userId:N}/{Guid.NewGuid():N}{GetExtension(fileName, contentType)}";
        var receipt = new Receipt
        {
            UserId = userId,
            FileName = Path.GetFileName(fileName),
            BlobKey = blobKey,
            FileUrl = string.Empty,
            ContentType = contentType,
            Size = size,
            Status = ReceiptStatus.Pending
        };

        try
        {
            receipt.FileUrl = await _blobStore.UploadAsync(content, blobKey, contentType);
            receipt.Status = ReceiptStatus.Uploaded;
        }
        catch
        {
            receipt.Status = ReceiptStatus.Failed;
            await _receiptRepository.AddAsync(receipt);
            await _receiptRepository.SaveChangesAsync();
            throw new BadRequestException("Failed to upload receipt to storage.");
        }

        await _receiptRepository.AddAsync(receipt);
        await _receiptRepository.SaveChangesAsync();
        return _mapper.Map<ReceiptDto>(receipt);
    }

    public async Task<ReceiptDto> GetAsync(Guid id, Guid userId)
    {
        var receipt = await GetOwnedOrThrowAsync(id, userId);
        return _mapper.Map<ReceiptDto>(receipt);
    }

    public async Task<IEnumerable<ReceiptDto>> ListByUserAsync(Guid userId)
    {
        var receipts = await _receiptRepository.ListByUserAsync(userId);
        return _mapper.Map<IEnumerable<ReceiptDto>>(receipts);
    }

    public async Task<ReceiptFileDto> GetFileAsync(Guid id, Guid userId)
    {
        var receipt = await GetOwnedOrThrowAsync(id, userId);
        if (receipt.Status != ReceiptStatus.Uploaded)
            throw new BadRequestException("Receipt file is not available.");

        var stream = await _blobStore.DownloadAsync(receipt.BlobKey);
        return new ReceiptFileDto
        {
            Content = stream,
            ContentType = receipt.ContentType,
            FileName = receipt.FileName
        };
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var receipt = await GetOwnedOrThrowAsync(id, userId);

        await _receiptRepository.ClearExpenseLinksAsync(id);

        try
        {
            await _blobStore.DeleteAsync(receipt.BlobKey);
        }
        catch
        {
            // Soft-delete DB even if blob cleanup fails
        }

        _receiptRepository.Delete(receipt);
        await _receiptRepository.SaveChangesAsync();
    }

    private async Task<Receipt> GetOwnedOrThrowAsync(Guid id, Guid userId)
    {
        var receipt = await _receiptRepository.GetOwnedAsync(id, userId);
        if (receipt == null)
            throw new NotFoundException(nameof(Receipt), id);

        return receipt;
    }

    private static void ValidateFile(string fileName, string contentType, long size)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new BadRequestException("File name is required.");

        if (size <= 0)
            throw new BadRequestException("File is empty.");

        if (size > MaxSizeBytes)
            throw new BadRequestException("File must be 5 MB or smaller.");

        if (!AllowedTypes.Contains(contentType))
            throw new BadRequestException("Only JPEG, PNG, WebP, and PDF files are allowed.");
    }

    private static string GetExtension(string fileName, string contentType)
    {
        var ext = Path.GetExtension(fileName);
        if (!string.IsNullOrWhiteSpace(ext))
            return ext.ToLowerInvariant();

        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "application/pdf" => ".pdf",
            _ => string.Empty
        };
    }
}
