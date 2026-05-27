using Serilog.Context;

namespace Aurora.API.Middlewares;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var header)
            && !string.IsNullOrWhiteSpace(header)
            ? header.ToString()
            : context.TraceIdentifier;

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
