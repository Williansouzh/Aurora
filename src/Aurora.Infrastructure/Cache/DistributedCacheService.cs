using Aurora.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace Aurora.Infrastructure.Cache;
public class DistributedCacheService(IDistributedCache cache, IConnectionMultiplexer? redis = null): ICacheService {
 public async Task<T?> GetAsync<T>(string key, CancellationToken ct=default){var s=await cache.GetStringAsync(key,ct); return s is null?default:JsonSerializer.Deserialize<T>(s);}
 public Task SetAsync<T>(string key,T value,TimeSpan ttl,CancellationToken ct=default)=>cache.SetStringAsync(key,JsonSerializer.Serialize(value),new DistributedCacheEntryOptions{AbsoluteExpirationRelativeToNow=ttl},ct);
 public async Task RemoveByPrefixAsync(string prefix,CancellationToken ct=default){
  if(redis is null) return;
  foreach(var endpoint in redis.GetEndPoints()){
   var server = redis.GetServer(endpoint);
   if(!server.IsConnected) continue;
   await foreach(var key in server.KeysAsync(pattern:$"{prefix}*")) await cache.RemoveAsync(key!,ct);
  }
 }
}
