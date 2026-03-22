using System.Net;
using System.Text.Json;
using ExpenseManager.Application.Exceptions;

namespace ExpenseManager.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            NotFoundException => (int)HttpStatusCode.NotFound,
            ConflictException => (int)HttpStatusCode.Conflict,
            UnauthorizedException => (int)HttpStatusCode.Unauthorized,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, 
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = statusCode;

        var message = statusCode == (int)HttpStatusCode.InternalServerError
            ? "An internal server error occurred."
            : exception.Message;

        var result = JsonSerializer.Serialize(new
        {
            statusCode,
            message
        });

        await context.Response.WriteAsync(result);
    }
}
