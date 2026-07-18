using AutoMapper;
using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Exceptions;
using ExpenseManager.Application.Interfaces;
using ExpenseManager.Application.Repositories;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Enums;
using System.Text.Json;

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

    private static readonly Guid FoodCategoryId = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000001");
    private static readonly Guid TransportCategoryId = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000002");
    private static readonly Guid HealthCategoryId = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000003");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IReceiptRepository _receiptRepository;
    private readonly IBlobStore _blobStore;
    private readonly IOcrService _ocrService;
    private readonly IReceiptOcrQueue _ocrQueue;
    private readonly IExpenseService _expenseService;
    private readonly IMapper _mapper;

    public ReceiptService(
        IReceiptRepository receiptRepository,
        IBlobStore blobStore,
        IOcrService ocrService,
        IReceiptOcrQueue ocrQueue,
        IExpenseService expenseService,
        IMapper mapper)
    {
        _receiptRepository = receiptRepository;
        _blobStore = blobStore;
        _ocrService = ocrService;
        _ocrQueue = ocrQueue;
        _expenseService = expenseService;
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
        await _ocrQueue.EnqueueAsync(receipt.Id);
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
        if (receipt.Status != ReceiptStatus.Uploaded
            && receipt.Status != ReceiptStatus.Processing
            && receipt.Status != ReceiptStatus.ReadyForReview
            && receipt.Status != ReceiptStatus.Confirmed
            && receipt.Status != ReceiptStatus.OcrFailed)
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

    public async Task ProcessOcrAsync(Guid receiptId)
    {
        var receipt = await _receiptRepository.GetByIdAsync(receiptId);
        if (receipt == null || receipt.IsDeleted)
            return;

        if (receipt.Status != ReceiptStatus.Uploaded && receipt.Status != ReceiptStatus.Processing)
            return;

        receipt.Status = ReceiptStatus.Processing;
        receipt.OcrErrorMessage = null;
        await _receiptRepository.SaveChangesAsync();

        try
        {
            await using var stream = await _blobStore.DownloadAsync(receipt.BlobKey);
            var result = await _ocrService.AnalyzeReceiptAsync(stream, receipt.ContentType);

            var lineItems = ApplyCategorySuggestions(result.Items ?? []);
            if (lineItems.Count == 0 && result.TotalAmount.HasValue)
            {
                lineItems.Add(new ReceiptLineItemDto
                {
                    Description = result.Merchant,
                    Quantity = 1,
                    TotalPrice = result.TotalAmount,
                    SuggestedCategoryId = SuggestCategoryId(result.Merchant)
                });
            }

            receipt.Merchant = result.Merchant;
            receipt.TransactionDate = result.TransactionDate;
            receipt.TotalAmount = result.TotalAmount;
            receipt.Currency = result.Currency?.ToUpperInvariant();
            receipt.TaxAmount = result.TaxAmount;
            receipt.SuggestedCategoryId = SuggestCategoryId(result.Merchant);
            receipt.LineItemsJson = SerializeLineItems(lineItems);
            receipt.OcrProcessedAt = DateTimeOffset.UtcNow;
            receipt.Status = ReceiptStatus.ReadyForReview;
        }
        catch (Exception ex)
        {
            receipt.Status = ReceiptStatus.OcrFailed;
            receipt.OcrErrorMessage = ex.Message;
            receipt.OcrProcessedAt = DateTimeOffset.UtcNow;
        }

        await _receiptRepository.SaveChangesAsync();
    }

    public async Task<ReceiptExtractionDto> GetExtractionAsync(Guid id, Guid userId)
    {
        var receipt = await GetOwnedOrThrowAsync(id, userId);
        return MapExtraction(receipt);
    }

    public async Task<IEnumerable<ExpenseResponseDto>> ConfirmExtractionAsync(
        Guid id,
        ConfirmReceiptExtractionDto dto,
        Guid userId)
    {
        var receipt = await _receiptRepository.GetOwnedWithExpensesAsync(id, userId);
        if (receipt == null)
            throw new NotFoundException(nameof(Receipt), id);

        if (receipt.Status != ReceiptStatus.ReadyForReview)
            throw new BadRequestException("Receipt extraction is not ready for confirmation.");

        if (receipt.Expenses.Count > 0)
            throw new BadRequestException("This receipt has already been confirmed.");

        var createDtos = dto.Expenses.Select(item => new CreateExpenseDto
        {
            Amount = item.Amount,
            Currency = dto.Currency,
            CategoryId = item.CategoryId,
            Date = dto.Date,
            Description = item.Description,
            ReceiptId = receipt.Id
        }).ToList();

        var created = (await _expenseService.CreateBatchAsync(createDtos, userId)).ToList();

        receipt.Status = ReceiptStatus.Confirmed;
        receipt.ConfirmedImportMode = dto.ImportMode;
        _receiptRepository.Update(receipt);
        await _receiptRepository.SaveChangesAsync();

        return created;
    }

    public async Task RetryOcrAsync(Guid id, Guid userId)
    {
        var receipt = await GetOwnedOrThrowAsync(id, userId);

        if (receipt.Status != ReceiptStatus.OcrFailed)
            throw new BadRequestException("Only failed OCR receipts can be retried.");

        receipt.Merchant = null;
        receipt.TransactionDate = null;
        receipt.TotalAmount = null;
        receipt.Currency = null;
        receipt.TaxAmount = null;
        receipt.SuggestedCategoryId = null;
        receipt.LineItemsJson = null;
        receipt.OcrProcessedAt = null;
        receipt.OcrErrorMessage = null;
        receipt.Status = ReceiptStatus.Uploaded;

        await _receiptRepository.SaveChangesAsync();
        await _ocrQueue.EnqueueAsync(receipt.Id);
    }

    private async Task<Receipt> GetOwnedOrThrowAsync(Guid id, Guid userId)
    {
        var receipt = await _receiptRepository.GetOwnedAsync(id, userId);
        if (receipt == null)
            throw new NotFoundException(nameof(Receipt), id);

        return receipt;
    }

    private static ReceiptExtractionDto MapExtraction(Receipt receipt)
    {
        return new ReceiptExtractionDto
        {
            ReceiptId = receipt.Id,
            Status = receipt.Status,
            Merchant = receipt.Merchant,
            TransactionDate = receipt.TransactionDate,
            TotalAmount = receipt.TotalAmount,
            Currency = receipt.Currency,
            TaxAmount = receipt.TaxAmount,
            SuggestedCategoryId = receipt.SuggestedCategoryId,
            OcrErrorMessage = receipt.OcrErrorMessage,
            LineItems = DeserializeLineItems(receipt.LineItemsJson)
        };
    }

    private static List<ReceiptLineItemDto> ApplyCategorySuggestions(IEnumerable<ReceiptLineItemDto> items)
    {
        return items.Select(item =>
        {
            item.SuggestedCategoryId ??= SuggestCategoryId(item.Description);
            return item;
        }).ToList();
    }

    private static string? SerializeLineItems(List<ReceiptLineItemDto> items)
    {
        return items.Count == 0 ? null : JsonSerializer.Serialize(items, JsonOptions);
    }

    private static List<ReceiptLineItemDto> DeserializeLineItems(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<List<ReceiptLineItemDto>>(json, JsonOptions) ?? [];
    }

    private static Guid? SuggestCategoryId(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var normalized = text.ToLowerInvariant();

        if (ContainsAny(normalized, "coffee", "cafe", "restaurant", "grocery", "food", "starbucks", "mcdonald", "pizza", "dining", "latte", "sandwich"))
            return FoodCategoryId;

        if (ContainsAny(normalized, "uber", "lyft", "taxi", "fuel", "gas", "shell", "bp", "parking", "transit", "transport"))
            return TransportCategoryId;

        if (ContainsAny(normalized, "pharmacy", "hospital", "clinic", "medical", "health", "cvs", "walgreens"))
            return HealthCategoryId;

        return null;
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        return keywords.Any(text.Contains);
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
