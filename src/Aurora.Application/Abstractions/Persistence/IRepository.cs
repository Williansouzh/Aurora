using Aurora.Domain.Common;

namespace Aurora.Application.Abstractions.Persistence;

/// <summary>
/// Generic CRUD contract for aggregates that belong to a single user (multi-tenant).
/// Concrete repositories may expose additional query methods.
/// </summary>
public interface IRepository<T> where T : EntityBase
{
    Task<T?> GetByIdAsync(string id, string userId, CancellationToken ct = default);
    Task<List<T>> GetByUserAsync(string userId, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(string id, string userId, CancellationToken ct = default);
}
