namespace ExpenseManager.Application.Interfaces;

public interface IBlobStore
{
    Task<string> UploadAsync(Stream content, string blobKey, string contentType, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string blobKey, CancellationToken ct = default);
    Task DeleteAsync(string blobKey, CancellationToken ct = default);
}
