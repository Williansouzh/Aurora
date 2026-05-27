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
    private readonly UnitOfWork.MongoUnitOfWork? _unitOfWork;

    protected MongoRepositoryBase(IMongoCollection<T> collection, UnitOfWork.MongoUnitOfWork? unitOfWork = null)
    {
        Collection = collection;
        _unitOfWork = unitOfWork;
    }

    public virtual async Task<T?> GetByIdAsync(string id, string userId, CancellationToken ct = default)
    {
        if (_unitOfWork?.CurrentSession is { } session)
        {
            return await Collection.Find(session, x => x.Id == id && x.UserId == userId)
                .FirstOrDefaultAsync(ct);
        }

        return await Collection.Find(x => x.Id == id && x.UserId == userId)
            .FirstOrDefaultAsync(ct);
    }

    public virtual Task<List<T>> GetByUserAsync(string userId, CancellationToken ct = default) =>
        _unitOfWork?.CurrentSession is { } session
            ? Collection.Find(session, x => x.UserId == userId).ToListAsync(ct)
            : Collection.Find(x => x.UserId == userId).ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
    {
        if (_unitOfWork?.CurrentSession is { } session)
        {
            await Collection.InsertOneAsync(session, entity, cancellationToken: ct);
        }
        else
        {
            await Collection.InsertOneAsync(entity, cancellationToken: ct);
        }

        _unitOfWork?.EnqueueDomainEvents(entity);
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        if (_unitOfWork?.CurrentSession is { } session)
        {
            await Collection.ReplaceOneAsync(
                session,
                x => x.Id == entity.Id && x.UserId == entity.UserId,
                entity,
                cancellationToken: ct);
        }
        else
        {
            await Collection.ReplaceOneAsync(
                x => x.Id == entity.Id && x.UserId == entity.UserId,
                entity,
                cancellationToken: ct);
        }

        _unitOfWork?.EnqueueDomainEvents(entity);
    }

    public virtual Task DeleteAsync(string id, string userId, CancellationToken ct = default) =>
        _unitOfWork?.CurrentSession is { } session
            ? Collection.DeleteOneAsync(session, x => x.Id == id && x.UserId == userId, cancellationToken: ct)
            : Collection.DeleteOneAsync(x => x.Id == id && x.UserId == userId, ct);
}
