using System.Net;
using System.Text.Json;

namespace Clean.Architecture.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
            _logger.LogError(exception, "An unexpected error occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/problem+json";

        var statusCode = GetStatusCodeFromException(exception);
        var (errorCode, errorMessage) = ExtractErrorDetails(exception);

        response.StatusCode = (int)statusCode;

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Title = GetTitleFromStatusCode(statusCode),
            Status = (int)statusCode,
            Detail = errorMessage,
            Instance = context.Request.Path.Value
        };

        // Add custom fields using Extensions
        problemDetails.Extensions["code"] = errorCode;
        problemDetails.Extensions["message"] = errorMessage;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        var jsonResponse = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await response.WriteAsync(jsonResponse);
    }

    private static HttpStatusCode GetStatusCodeFromException(Exception exception)
    {
        return exception switch
        {
            InvalidOperationException ex when ex.Message.Contains("NotFound") => HttpStatusCode.NotFound,
            InvalidOperationException ex when ex.Message.Contains("Invalid") => HttpStatusCode.BadRequest,
            InvalidOperationException ex when ex.Message.Contains("Conflict") => HttpStatusCode.Conflict,
            ArgumentException or ArgumentNullException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };
    }

    private static string GetTitleFromStatusCode(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.NotFound => "Not Found",
            HttpStatusCode.BadRequest => "Bad Request",
            HttpStatusCode.Conflict => "Conflict",
            HttpStatusCode.Unauthorized => "Unauthorized",
            HttpStatusCode.Forbidden => "Forbidden",
            _ => "Internal Server Error"
        };
    }

    private static (string code, string message) ExtractErrorDetails(Exception exception)
    {
        // Fallback for exception types
        return ("Server.InternalError", exception.Message);
    }
}
