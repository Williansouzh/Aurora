namespace Aurora.Application.Abstractions.Common;

public interface IRateLimiter
{
    Task<bool> IsAllowedAsync(string key, int maxRequests, TimeSpan window);
}
