using Aurora.Application.Abstractions.Messaging;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Common;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.UnitOfWork;

public class MongoUnitOfWork(MongoContext context, IDomainEventDispatcher dispatcher) : IUnitOfWork, IDisposable
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IClientSessionHandle? CurrentSession { get; private set; }

    public bool HasActiveTransaction => CurrentSession is { IsInTransaction: true };

    public void EnqueueDomainEvents(EntityBase entity)
    {
        if (entity.DomainEvents.Count == 0)
        {
            return;
        }

        _domainEvents.AddRange(entity.DomainEvents);
        entity.ClearDomainEvents();
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
    {
        if (HasActiveTransaction)
        {
            await operation(ct);
            return;
        }

        using var session = await context.Client.StartSessionAsync(cancellationToken: ct);
        CurrentSession = session;

        try
        {
            session.StartTransaction();
            await operation(ct);
            await session.CommitTransactionAsync(ct);
            await DispatchEventsAsync(ct);
        }
        catch
        {
            if (session.IsInTransaction)
            {
                await session.AbortTransactionAsync(ct);
            }

            throw;
        }
        finally
        {
            CurrentSession = null;
            _domainEvents.Clear();
        }
    }

    public void Dispose()
    {
        CurrentSession?.Dispose();
    }

    private async Task DispatchEventsAsync(CancellationToken ct)
    {
        if (_domainEvents.Count == 0)
        {
            return;
        }

        var events = _domainEvents.ToArray();
        _domainEvents.Clear();
        await dispatcher.DispatchAsync(events, ct);
    }
}
