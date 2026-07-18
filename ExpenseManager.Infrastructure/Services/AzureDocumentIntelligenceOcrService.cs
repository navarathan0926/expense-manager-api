using Azure;
using Azure.AI.DocumentIntelligence;
using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ExpenseManager.Infrastructure.Services;

public class AzureDocumentIntelligenceOcrService : IOcrService
{
    private readonly DocumentIntelligenceClient _client;

    public AzureDocumentIntelligenceOcrService(IConfiguration configuration)
    {
        var endpoint = configuration["AzureDocumentIntelligence:Endpoint"]
            ?? throw new InvalidOperationException("AzureDocumentIntelligence:Endpoint is not configured.");
        var apiKey = configuration["AzureDocumentIntelligence:ApiKey"]
            ?? throw new InvalidOperationException("AzureDocumentIntelligence:ApiKey is not configured.");

        _client = new DocumentIntelligenceClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
    }

    public async Task<ReceiptOcrResultDto> AnalyzeReceiptAsync(Stream content, string contentType, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms, ct);
        ms.Position = 0;

        var options = new AnalyzeDocumentOptions("prebuilt-receipt", BinaryData.FromBytes(ms.ToArray()));
        var operation = await _client.AnalyzeDocumentAsync(WaitUntil.Completed, options, ct);
        var document = operation.Value.Documents.FirstOrDefault()
            ?? throw new InvalidOperationException("No document was returned from OCR analysis.");

        document.Fields.TryGetValue("MerchantName", out var merchantField);
        document.Fields.TryGetValue("TransactionDate", out var dateField);
        document.Fields.TryGetValue("Total", out var totalField);
        document.Fields.TryGetValue("Tax", out var taxField);
        document.Fields.TryGetValue("Items", out var itemsField);

        return new ReceiptOcrResultDto
        {
            Merchant = merchantField?.ValueString ?? merchantField?.Content,
            TransactionDate = dateField?.ValueDate,
            TotalAmount = totalField?.ValueCurrency != null
                ? (decimal)totalField.ValueCurrency.Amount
                : null,
            Currency = totalField?.ValueCurrency?.CurrencyCode,
            TaxAmount = taxField?.ValueCurrency != null
                ? (decimal)taxField.ValueCurrency.Amount
                : null,
            Items = ParseLineItems(itemsField)
        };
    }

    private static List<ReceiptLineItemDto> ParseLineItems(DocumentField? itemsField)
    {
        var items = new List<ReceiptLineItemDto>();
        if (itemsField?.ValueList == null)
            return items;

        foreach (var itemField in itemsField.ValueList)
        {
            if (itemField.ValueDictionary == null)
                continue;

            itemField.ValueDictionary.TryGetValue("Description", out var descriptionField);
            itemField.ValueDictionary.TryGetValue("Quantity", out var quantityField);
            itemField.ValueDictionary.TryGetValue("Price", out var priceField);
            itemField.ValueDictionary.TryGetValue("TotalPrice", out var totalPriceField);

            var totalPrice = totalPriceField?.ValueCurrency != null
                ? (decimal?)totalPriceField.ValueCurrency.Amount
                : null;
            var unitPrice = priceField?.ValueCurrency != null
                ? (decimal?)priceField.ValueCurrency.Amount
                : null;

            items.Add(new ReceiptLineItemDto
            {
                Description = descriptionField?.ValueString ?? descriptionField?.Content,
                Quantity = quantityField?.ValueDouble != null ? (decimal)quantityField.ValueDouble : null,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice ?? unitPrice
            });
        }

        return items;
    }
}
