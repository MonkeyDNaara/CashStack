using CashStack.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CashStack.Api.ErrorHandling;

// Turns unhandled exceptions into ProblemDetails responses without internal details
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        (int status, string title, string detail) = Map(exception);

        // The full exception (message + stack trace) goes to the log only, never to the client
        _logger.Log(
            status >= StatusCodes.Status500InternalServerError ? LogLevel.Error : LogLevel.Warning,
            exception,
            "Request {Method} {Path} failed with {StatusCode}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            status);

        httpContext.Response.StatusCode = status;
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            }
        });
    }

    private static (int Status, string Title, string Detail) Map(Exception exception)
    {
        return exception switch
        {
            // Domain messages are written by us and safe to show, e.g. "A category named 'Rent' already exists."
            // Matching the base types means new rules need no change here (polymorphism)
            ConflictException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message),
            BusinessRuleException => (
                StatusCodes.Status400BadRequest,
                "Business rule violated",
                exception.Message),
            StorageException => (
                StatusCodes.Status500InternalServerError,
                "Storage error",
                "The data could not be read or saved."),
            // Also covers ArgumentOutOfRangeException (derives from ArgumentException)
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                "The request contains invalid values."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "Please try again later.")
        };
    }
}
