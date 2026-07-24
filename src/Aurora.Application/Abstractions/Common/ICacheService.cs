namespace Aurora.Application.Abstractions.Common;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);

    /// <summary>Atomically sets <paramref name="key"/> only if absent (SET NX). Returns true if acquired.</summary>
    Task<bool> AcquireLockAsync(string key, TimeSpan ttl, CancellationToken ct = default);
}
