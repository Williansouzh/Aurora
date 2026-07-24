using System.Text.Json;
using Aurora.Application.Abstractions.Common;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Aurora.Infrastructure.Cache;

public class RedisCacheService(IDistributedCache cache, IConnectionMultiplexer? redis = null) : ICacheService
{
    // Index sets let us invalidate a group of keys without ever scanning the whole keyspace.
    // Dashboard cache keys are structured "aurora:dashboard:{userId}:..." and are always
    // invalidated per-user via RemoveByPrefixAsync(DashboardPrefix(userId)); we track each written
    // key in a per-user set so removal is an O(keys-for-user) operation instead of O(keyspace).
    private const string IndexPrefix = "idx:";

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var s = await cache.GetStringAsync(key, ct);
        return s is null ? default : JsonSerializer.Deserialize<T>(s);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        await cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
            ct);

        var indexKey = IndexKeyFor(key);
        if (redis is not null && indexKey is not null)
        {
            var db = redis.GetDatabase();
            await db.SetAddAsync(indexKey, key);
            // Keep the index alive a bit longer than any member so it never expires mid-use.
            await db.KeyExpireAsync(indexKey, ttl + TimeSpan.FromMinutes(10));
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        if (redis is null) return;

        var db = redis.GetDatabase();
        var indexKey = $"{IndexPrefix}{prefix}";
        var members = await db.SetMembersAsync(indexKey);

        foreach (var member in members)
        {
            await cache.RemoveAsync(member!, ct);
        }

        await db.KeyDeleteAsync(indexKey);
    }

    private static string? IndexKeyFor(string key)
    {
        // "aurora:dashboard:{userId}:..." -> "idx:aurora:dashboard:{userId}", matching the prefix
        // that RemoveByPrefixAsync is called with.
        var parts = key.Split(':');
        if (parts.Length >= 3 && parts[0] == "aurora" && parts[1] == "dashboard")
        {
            return $"{IndexPrefix}{parts[0]}:{parts[1]}:{parts[2]}";
        }

        return null;
    }
}
