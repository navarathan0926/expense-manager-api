namespace ExpenseManager.Application.DTOs;

public class ReceiptFileDto
{
    public Stream Content { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
}
