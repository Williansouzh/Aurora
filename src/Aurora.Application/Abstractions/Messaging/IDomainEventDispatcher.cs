using Aurora.Domain.Common;

namespace Aurora.Application.Abstractions.Messaging;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default);
}
