using Aurora.Domain.Entities;
namespace Aurora.Application.Interfaces;
public interface IUserContext { string UserId { get; } }
public interface IJwtTokenService { string Generate(User user); }
public interface IPasswordHasher { string Hash(string password); bool Verify(string password, string hash); }
public interface ICacheService { Task<T?> GetAsync<T>(string key,CancellationToken ct=default); Task SetAsync<T>(string key,T value,TimeSpan ttl,CancellationToken ct=default); Task RemoveByPrefixAsync(string prefix,CancellationToken ct=default); }
public interface IRepository<T> { Task<T?> GetByIdAsync(string id); Task<List<T>> FilterAsync(Func<T,bool> pred); Task InsertAsync(T entity); Task UpdateAsync(string id,T entity); Task DeleteAsync(string id); }
