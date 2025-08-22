using System.Net;
using System.Text.Json;
using Hypesoft.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Infrastructure.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger
    )
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
        _logger.LogError(exception, "An unexpected error occurred");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)GetStatusCode(exception);

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = GetErrorMessage(exception),
            Details = exception is ApiException ? exception.Message : null,
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);

        await context.Response.WriteAsync(jsonResponse);
    }

    private static HttpStatusCode GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ApiException apiEx => apiEx.StatusCode,
            UnauthorizedAccessException _ => HttpStatusCode.Unauthorized,
            _ => HttpStatusCode.InternalServerError,
        };
    }

    private static string GetErrorMessage(Exception exception)
    {
        return exception is ApiException
            ? "An error occurred while processing your request."
            : "An unexpected error occurred. Please try again later.";
    }
}
