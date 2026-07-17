using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Exceptions;
using ExpenseManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

[Authorize]
public class ReceiptController : BaseController
{
    private readonly IReceiptService _receiptService;

    public ReceiptController(IReceiptService receiptService)
    {
        _receiptService = receiptService;
    }

    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<ReceiptDto>> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new BadRequestException("File is required.");

        await using var stream = file.OpenReadStream();
        var created = await _receiptService.UploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            CurrentUserId);

        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReceiptDto>>> GetAll()
    {
        var receipts = await _receiptService.ListByUserAsync(CurrentUserId);
        return Ok(receipts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReceiptDto>> GetById(Guid id)
    {
        var receipt = await _receiptService.GetAsync(id, CurrentUserId);
        return Ok(receipt);
    }

    [HttpGet("{id:guid}/file")]
    public async Task<IActionResult> GetFile(Guid id)
    {
        var file = await _receiptService.GetFileAsync(id, CurrentUserId);
        Response.Headers.ContentDisposition = $"inline; filename=\"{file.FileName}\"";
        return File(file.Content, file.ContentType);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _receiptService.DeleteAsync(id, CurrentUserId);
        return NoContent();
    }
}
