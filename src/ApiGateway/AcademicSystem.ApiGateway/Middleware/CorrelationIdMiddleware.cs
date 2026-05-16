namespace AcademicSystem.ApiGateway.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string? correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            context.Request.Headers["X-Correlation-Id"] = correlationId;
        }
        
        context.Response.Headers["X-Correlation-Id"] = correlationId;
        
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Path"] = context.Request.Path.ToString(),
            ["Method"] = context.Request.Method
        }))
        {
            _logger.LogInformation("Processing request: {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            
            await _next(context);
            
            _logger.LogInformation("Request completed with status: {StatusCode}", 
                context.Response.StatusCode);
        }
    }
}