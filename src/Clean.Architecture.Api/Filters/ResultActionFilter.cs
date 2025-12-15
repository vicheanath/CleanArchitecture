using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Results;
using System.Net;

namespace Clean.Architecture.Api.Filters;

/// <summary>
/// Action filter that automatically converts Result and Result&lt;T&gt; return types to appropriate HTTP responses.
/// </summary>
public class ResultActionFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // Handle CreatedAtActionResult with Result value
        if (context.Result is CreatedAtActionResult createdAtAction && createdAtAction.Value is Result createdResult)
        {
            if (createdResult.IsFailure)
            {
                var statusCode = GetStatusCodeFromError(createdResult.Error);
                var problemDetails = CreateProblemDetails(createdResult.Error, statusCode, context.HttpContext);
                context.Result = new ObjectResult(problemDetails)
                {
                    StatusCode = (int)statusCode
                };
            }
            else if (createdResult.GetType().IsGenericType)
            {
                var valueProperty = createdResult.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    var value = valueProperty.GetValue(createdResult);
                    context.Result = new CreatedAtActionResult(
                        createdAtAction.ActionName,
                        createdAtAction.ControllerName,
                        createdAtAction.RouteValues,
                        value);
                }
            }
        }
        // Handle ObjectResult with Result value
        else if (context.Result is ObjectResult objectResult)
        {
            // Handle Result<T>
            if (objectResult.Value is Result result && result.GetType().IsGenericType)
            {
                var resultType = result.GetType();
                var valueProperty = resultType.GetProperty("Value");

                if (result.IsFailure)
                {
                    var statusCode = GetStatusCodeFromError(result.Error);
                    var problemDetails = CreateProblemDetails(result.Error, statusCode, context.HttpContext);
                    context.Result = new ObjectResult(problemDetails)
                    {
                        StatusCode = (int)statusCode
                    };
                }
                else if (valueProperty != null)
                {
                    // Success case - extract the value
                    var value = valueProperty.GetValue(result);
                    context.Result = new ObjectResult(value)
                    {
                        StatusCode = objectResult.StatusCode ?? 200
                    };
                }
            }
            // Handle Result (non-generic)
            else if (objectResult.Value is Result nonGenericResult && !nonGenericResult.GetType().IsGenericType)
            {
                if (nonGenericResult.IsFailure)
                {
                    var statusCode = GetStatusCodeFromError(nonGenericResult.Error);
                    var problemDetails = CreateProblemDetails(nonGenericResult.Error, statusCode, context.HttpContext);
                    context.Result = new ObjectResult(problemDetails)
                    {
                        StatusCode = (int)statusCode
                    };
                }
                // Success case for non-generic Result - remove the Result wrapper, keep original status
                // The controller should have already set the appropriate status (NoContent, etc.)
                // So we just need to remove the Result from the response
                else if (context.Result is ObjectResult orResult)
                {
                    // If it's a successful Result, we should have already handled it in the controller
                    // by returning NoContent() or similar. But if we get here, just remove the Result wrapper.
                    context.Result = new StatusCodeResult(orResult.StatusCode ?? 200);
                }
            }
        }

        await next();
    }

    private static HttpStatusCode GetStatusCodeFromError(Shared.Errors.Error error)
    {
        if (error.Code.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
            return HttpStatusCode.NotFound;

        if (error.Code.Contains("Conflict", StringComparison.OrdinalIgnoreCase))
            return HttpStatusCode.Conflict;

        if (error.Code.Contains("Invalid", StringComparison.OrdinalIgnoreCase))
            return HttpStatusCode.BadRequest;

        return HttpStatusCode.InternalServerError;
    }

    private static Microsoft.AspNetCore.Mvc.ProblemDetails CreateProblemDetails(
        Shared.Errors.Error error,
        HttpStatusCode statusCode,
        HttpContext httpContext)
    {
        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Title = GetTitleFromStatusCode(statusCode),
            Status = (int)statusCode,
            Detail = error.Message,
            Instance = httpContext.Request.Path.Value
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
}
