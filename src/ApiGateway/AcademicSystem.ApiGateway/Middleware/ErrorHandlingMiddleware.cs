using System.Net;
using System.Text.Json;

namespace AcademicSystem.ApiGateway.Middleware;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        
        var (statusCode, title, detail) = exception switch
        {
            UnauthorizedAccessException => 
                ((int)HttpStatusCode.Unauthorized, "Unauthorized", exception.Message),
            KeyNotFoundException => 
                ((int)HttpStatusCode.NotFound, "Not Found", exception.Message),
            ArgumentException or InvalidOperationException => 
                ((int)HttpStatusCode.BadRequest, "Bad Request", exception.Message),
            _ => 
                ((int)HttpStatusCode.InternalServerError, "Internal Server Error", "An error occurred while processing your request")
        };

        context.Response.StatusCode = statusCode;
        
        var response = new
        {
            title,
            status = statusCode,
            detail,
            timestamp = DateTime.UtcNow,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}