using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Common;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic CRUD repository for aggregates that implement <see cref="IUserOwned"/>.
/// Every read and write is automatically scoped by <c>UserId</c> to prevent cross-tenant leaks.
/// </summary>
public abstract class MongoRepositoryBase<T> : IRepository<T>
    where T : EntityBase, IUserOwned
{
    protected readonly IMongoCollection<T> Collection;

    protected MongoRepositoryBase(IMongoCollection<T> collection)
    {
        Collection = collection;
    }

    public virtual Task<T?> GetByIdAsync(string id, string userId, CancellationToken ct = default) =>
        Collection.Find(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync(ct)!;

    public virtual Task<List<T>> GetByUserAsync(string userId, CancellationToken ct = default) =>
        Collection.Find(x => x.UserId == userId).ToListAsync(ct);

    public virtual Task AddAsync(T entity, CancellationToken ct = default) =>
        Collection.InsertOneAsync(entity, cancellationToken: ct);

    public virtual Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        return Collection.ReplaceOneAsync(
            x => x.Id == entity.Id && x.UserId == entity.UserId,
            entity,
            cancellationToken: ct);
    }

    public virtual Task DeleteAsync(string id, string userId, CancellationToken ct = default) =>
        Collection.DeleteOneAsync(x => x.Id == id && x.UserId == userId, ct);
}
