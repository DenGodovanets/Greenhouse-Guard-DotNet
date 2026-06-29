namespace GreenhouseGuard.Api.Middleware;

/// <summary>
///     Global exception handling middleware that catches all unhandled exceptions
///     and returns standardized ProblemDetails responses (RFC 7807).
///     Features:
///     - Converts exceptions to appropriate HTTP status codes
///     - Logs all exceptions with context
///     - Returns machine-readable error format
///     - Includes trace IDs for debugging
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(
            exception,
            "Unhandled exception: {ExceptionType} - {Message}",
            exception.GetType().Name,
            exception.Message);

        var response = context.Response;
        response.ContentType = "application/json";

        var problemDetails = MapExceptionToProblemDetails(exception, context);

        response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        return response.WriteAsJsonAsync(problemDetails);
    }

    /// <summary>
    ///     Maps exception types to appropriate HTTP status codes and error details.
    /// </summary>
    private static ProblemDetails MapExceptionToProblemDetails(Exception exception, HttpContext context)
    {
        var traceId = context.TraceIdentifier;

        return exception switch
        {
            // Validation Errors → 400 Bad Request
            ArgumentNullException or ArgumentOutOfRangeException => new ProblemDetails
            {
                Type = "https://api.example.com/errors/validation-failed",
                Title = "Validation Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = exception.Message,
                Instance = context.Request.Path,
                Extensions = { { "traceId", traceId } }
            },

            ArgumentException => new ProblemDetails
            {
                Type = "https://api.example.com/errors/validation-failed",
                Title = "Validation Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = exception.Message,
                Instance = context.Request.Path,
                Extensions = { { "traceId", traceId } }
            },

            // Operation Cancelled → 408 Request Timeout
            OperationCanceledException => new ProblemDetails
            {
                Type = "https://api.example.com/errors/operation-cancelled",
                Title = "Operation Cancelled",
                Status = StatusCodes.Status408RequestTimeout,
                Detail = "The operation was cancelled before completion.",
                Instance = context.Request.Path,
                Extensions = { { "traceId", traceId } }
            },

            // Not Found → 404
            KeyNotFoundException => new ProblemDetails
            {
                Type = "https://api.example.com/errors/not-found",
                Title = "Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = exception.Message,
                Instance = context.Request.Path,
                Extensions = { { "traceId", traceId } }
            },

            // Invalid Operation → 409 Conflict
            InvalidOperationException => new ProblemDetails
            {
                Type = "https://api.example.com/errors/invalid-operation",
                Title = "Invalid Operation",
                Status = StatusCodes.Status409Conflict,
                Detail = exception.Message,
                Instance = context.Request.Path,
                Extensions = { { "traceId", traceId } }
            },

            // Database Errors → 500 Internal Server Error
            _ => new ProblemDetails
            {
                Type = "https://api.example.com/errors/internal-server-error",
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred while processing your request.",
                Instance = context.Request.Path,
                Extensions = { { "traceId", traceId } }
            }
        };
    }
}

/// <summary>
///     Problem Details response format compliant with RFC 7807.
///     Used for standardized error responses across the API.
/// </summary>
public class ProblemDetails
{
    /// <summary>
    ///     A URI reference (RFC 3986) that identifies the problem type.
    ///     Should resolve to human-readable documentation.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    ///     A short, human-readable summary of the problem type.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    ///     The HTTP status code.
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    ///     A human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    ///     A URI reference that identifies the specific occurrence of the problem.
    ///     May be used to correlate errors with logs.
    /// </summary>
    public string? Instance { get; set; }

    /// <summary>
    ///     Additional properties specific to the problem (e.g., traceId, timestamp).
    /// </summary>
    public Dictionary<string, object?> Extensions { get; } = new(StringComparer.Ordinal);
}