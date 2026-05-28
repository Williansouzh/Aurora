using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Aurora.API.Health;

public class RedisHealthCheck(IConnectionMultiplexer redis) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var pong = await redis.GetDatabase().PingAsync();
        return pong >= TimeSpan.Zero ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    }
}
