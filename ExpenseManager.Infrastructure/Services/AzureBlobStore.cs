using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ExpenseManager.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ExpenseManager.Infrastructure.Services;

public class AzureBlobStore : IBlobStore
{
    private readonly BlobContainerClient _container;

    public AzureBlobStore(IConfiguration configuration)
    {
        var connectionString = configuration["AzureBlob:ConnectionString"]
            ?? throw new InvalidOperationException("AzureBlob:ConnectionString is not configured.");
        var containerName = configuration["AzureBlob:Container"] ?? "receipts";

        var serviceClient = new BlobServiceClient(connectionString);
        _container = serviceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> UploadAsync(Stream content, string blobKey, string contentType, CancellationToken ct = default)
    {
        await _container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

        var blob = _container.GetBlobClient(blobKey);
        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, ct);
        return blob.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string blobKey, CancellationToken ct = default)
    {
        var blob = _container.GetBlobClient(blobKey);
        var response = await blob.DownloadStreamingAsync(cancellationToken: ct);
        return response.Value.Content;
    }

    public async Task DeleteAsync(string blobKey, CancellationToken ct = default)
    {
        var blob = _container.GetBlobClient(blobKey);
        await blob.DeleteIfExistsAsync(cancellationToken: ct);
    }
}
