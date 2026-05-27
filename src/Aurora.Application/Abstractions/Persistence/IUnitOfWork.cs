using Aurora.Domain.Common;

namespace Aurora.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    bool HasActiveTransaction { get; }
    void EnqueueDomainEvents(EntityBase entity);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default);
}
