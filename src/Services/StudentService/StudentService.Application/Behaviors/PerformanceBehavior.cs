using MediatR;
using System.Diagnostics;

namespace StudentService.Application.Behaviors;

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer;
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _timer = new Stopwatch();
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();
        var response = await next();
        _timer.Stop();
        
        var elapsedMilliseconds = _timer.ElapsedMilliseconds;
        var requestName = typeof(TRequest).Name;
        
        if (elapsedMilliseconds > 5000)
        {
            _logger.LogWarning("Long Running Request: {RequestName} ({ElapsedMilliseconds} milliseconds)", 
                requestName, elapsedMilliseconds);
        }
        else
        {
            _logger.LogInformation("Request: {RequestName} completed in {ElapsedMilliseconds}ms", 
                requestName, elapsedMilliseconds);
        }
        
        return response;
    }
}