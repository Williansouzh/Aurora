using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Aurora.Application.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > 500)
        {
            logger.LogWarning(
                "Request {RequestName} took {ElapsedMilliseconds}ms",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
