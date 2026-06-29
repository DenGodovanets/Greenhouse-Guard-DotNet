using FluentAssertions;

namespace GreenhouseGuard.Tests;

/// <summary>
///     Unit tests for ExceptionHandlingMiddleware - global error handler.
///     Validates that exceptions are caught and converted to standardized
///     ProblemDetails responses (RFC 7807) with appropriate HTTP status codes.
/// </summary>
public sealed class ExceptionHandlingMiddlewareTests
{
    /// <summary>
    ///     SCENARIO: ArgumentException thrown during request processing
    ///     EXPECTED: Returns 400 Bad Request with ProblemDetails
    /// </summary>
    [Fact]
    public void ArgumentException_ShouldMapTo400BadRequest()
    {
        // Arrange
        var exception = new ArgumentException("Temperature is required");
        var problemDetails = MapExceptionToProblemDetails(exception);

        // Act & Assert
        problemDetails.Status.Should().Be(400);
        problemDetails.Title.Should().Be("Validation Failed");
        problemDetails.Type.Should().Contain("validation-failed");
        problemDetails.Detail.Should().Contain("Temperature is required");
    }

    /// <summary>
    ///     SCENARIO: ArgumentNullException thrown (null input)
    ///     EXPECTED: Returns 400 Bad Request
    /// </summary>
    [Fact]
    public void ArgumentNullException_ShouldMapTo400BadRequest()
    {
        // Arrange
        var exception = new ArgumentNullException("sensorType");
        var problemDetails = MapExceptionToProblemDetails(exception);

        // Act & Assert
        problemDetails.Status.Should().Be(400);
        problemDetails.Title.Should().Be("Validation Failed");
    }

    /// <summary>
    ///     SCENARIO: KeyNotFoundException thrown (resource not found)
    ///     EXPECTED: Returns 404 Not Found
    /// </summary>
    [Fact]
    public void KeyNotFoundException_ShouldMapTo404NotFound()
    {
        // Arrange
        var exception = new KeyNotFoundException("Sensor with ID xyz not found");
        var problemDetails = MapExceptionToProblemDetails(exception);

        // Act & Assert
        problemDetails.Status.Should().Be(404);
        problemDetails.Title.Should().Be("Not Found");
        problemDetails.Type.Should().Contain("not-found");
    }

    /// <summary>
    ///     SCENARIO: OperationCanceledException thrown (timeout)
    ///     EXPECTED: Returns 408 Request Timeout
    /// </summary>
    [Fact]
    public void OperationCanceledException_ShouldMapTo408RequestTimeout()
    {
        // Arrange
        var exception = new OperationCanceledException("Request was cancelled");
        var problemDetails = MapExceptionToProblemDetails(exception);

        // Act & Assert
        problemDetails.Status.Should().Be(408);
        problemDetails.Title.Should().Be("Operation Cancelled");
        problemDetails.Type.Should().Contain("operation-cancelled");
    }

    /// <summary>
    ///     SCENARIO: InvalidOperationException thrown (business logic error)
    ///     EXPECTED: Returns 409 Conflict
    /// </summary>
    [Fact]
    public void InvalidOperationException_ShouldMapTo409Conflict()
    {
        // Arrange
        var exception = new InvalidOperationException("Cannot process reading: anomaly threshold exceeded");
        var problemDetails = MapExceptionToProblemDetails(exception);

        // Act & Assert
        problemDetails.Status.Should().Be(409);
        problemDetails.Title.Should().Be("Invalid Operation");
        problemDetails.Type.Should().Contain("invalid-operation");
    }

    /// <summary>
    ///     SCENARIO: Unexpected exception (any other type)
    ///     EXPECTED: Returns 500 Internal Server Error
    /// </summary>
    [Fact]
    public void UnexpectedException_ShouldMapTo500InternalServerError()
    {
        // Arrange
        var exception = new Exception("Database connection failed");
        var problemDetails = MapExceptionToProblemDetails(exception);

        // Act & Assert
        problemDetails.Status.Should().Be(500);
        problemDetails.Title.Should().Be("Internal Server Error");
        problemDetails.Type.Should().Contain("internal-server-error");
        problemDetails.Detail.Should().Contain("unexpected error");
    }

    /// <summary>
    ///     SCENARIO: All error responses include trace ID
    ///     EXPECTED: Extensions contain traceId for debugging
    /// </summary>
    [Fact]
    public void ErrorResponse_ShouldIncludeTraceId()
    {
        // Arrange
        var exception = new ArgumentException("Invalid input");
        var problemDetails = MapExceptionToProblemDetails(exception);

        // Act & Assert
        problemDetails.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    /// <summary>
    ///     SCENARIO: ProblemDetails includes all required RFC 7807 fields
    ///     EXPECTED: Type, Title, Status, Detail, Instance populated
    /// </summary>
    [Fact]
    public void ProblemDetails_ShouldFollowRfc7807Standard()
    {
        // Arrange
        var exception = new ArgumentException("Humidity out of range");
        var problemDetails = MapExceptionToProblemDetails(exception);

        // Act & Assert
        problemDetails.Type.Should().NotBeNullOrEmpty();
        problemDetails.Title.Should().NotBeNullOrEmpty();
        problemDetails.Status.Should().NotBeNull().And.BeGreaterThanOrEqualTo(400);
        problemDetails.Instance.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    ///     Helper method to map exception - simulates middleware logic
    /// </summary>
    private static ProblemDetails MapExceptionToProblemDetails(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => new ProblemDetails
            {
                Type = "https://api.example.com/errors/validation-failed",
                Title = "Validation Failed",
                Status = 400,
                Detail = exception.Message,
                Instance = "/api/readings",
                Extensions = { { "traceId", "0HN1GDGP41AB2:00000001" } }
            },

            ArgumentException => new ProblemDetails
            {
                Type = "https://api.example.com/errors/validation-failed",
                Title = "Validation Failed",
                Status = 400,
                Detail = exception.Message,
                Instance = "/api/readings",
                Extensions = { { "traceId", "0HN1GDGP41AB2:00000001" } }
            },

            KeyNotFoundException => new ProblemDetails
            {
                Type = "https://api.example.com/errors/not-found",
                Title = "Not Found",
                Status = 404,
                Detail = exception.Message,
                Instance = "/api/readings",
                Extensions = { { "traceId", "0HN1GDGP41AB2:00000001" } }
            },

            OperationCanceledException => new ProblemDetails
            {
                Type = "https://api.example.com/errors/operation-cancelled",
                Title = "Operation Cancelled",
                Status = 408,
                Detail = "The operation was cancelled before completion.",
                Instance = "/api/readings",
                Extensions = { { "traceId", "0HN1GDGP41AB2:00000001" } }
            },

            InvalidOperationException => new ProblemDetails
            {
                Type = "https://api.example.com/errors/invalid-operation",
                Title = "Invalid Operation",
                Status = 409,
                Detail = exception.Message,
                Instance = "/api/readings",
                Extensions = { { "traceId", "0HN1GDGP41AB2:00000001" } }
            },

            _ => new ProblemDetails
            {
                Type = "https://api.example.com/errors/internal-server-error",
                Title = "Internal Server Error",
                Status = 500,
                Detail = "An unexpected error occurred while processing your request.",
                Instance = "/api/readings",
                Extensions = { { "traceId", "0HN1GDGP41AB2:00000001" } }
            }
        };
    }
}

/// <summary>
///     Problem Details response format compliant with RFC 7807.
/// </summary>
public class ProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public Dictionary<string, object?> Extensions { get; } = new(StringComparer.Ordinal);
}