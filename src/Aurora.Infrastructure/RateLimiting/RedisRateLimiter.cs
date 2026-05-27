using Aurora.Application.Abstractions.Common;
using StackExchange.Redis;

namespace Aurora.Infrastructure.RateLimiting;

public class RedisRateLimiter(IConnectionMultiplexer redis) : IRateLimiter
{
    public async Task<bool> IsAllowedAsync(string key, int maxRequests, TimeSpan window)
    {
        var db = redis.GetDatabase();
        var rlKey = $"rl:{key}";
        var count = await db.StringIncrementAsync(rlKey);
        if (count == 1) await db.KeyExpireAsync(rlKey, window);
        return count <= maxRequests;
    }
}
