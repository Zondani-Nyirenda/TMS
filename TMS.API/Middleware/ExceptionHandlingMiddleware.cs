using System.Net;
using System.Text.Json;
using TMS.Domain.Exceptions;

namespace TMS.API.Middleware;

/// <summary>
/// Catches all unhandled exceptions and converts them to consistent JSON error responses.
/// Maps domain exceptions to appropriate HTTP status codes.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException e => (HttpStatusCode.NotFound, e.Message),
            ForbiddenException e => (HttpStatusCode.Forbidden, e.Message),
            ClassFullException e => (HttpStatusCode.UnprocessableEntity, e.Message),
            PaymentExceedsBalanceException e
                                       => (HttpStatusCode.UnprocessableEntity, e.Message),
            DomainException e => (HttpStatusCode.UnprocessableEntity, e.Message),
            UnauthorizedAccessException e
                                       => (HttpStatusCode.Unauthorized, e.Message),
            _ => (HttpStatusCode.InternalServerError,
                                           "An unexpected error occurred. Please try again.")
        };

        // Log server errors with full stack trace; client errors as warnings
        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            _logger.LogWarning("Handled exception [{StatusCode}]: {Message}",
                               (int)statusCode, exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            success = false,
            statusCode = (int)statusCode,
            message,
            errors = new[] { message },
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}