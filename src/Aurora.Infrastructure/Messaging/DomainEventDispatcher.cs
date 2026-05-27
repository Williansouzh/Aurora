using Aurora.Application.Abstractions.Messaging;
using Aurora.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Aurora.Infrastructure.Messaging;

public class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;
                var task = (Task)method.Invoke(handler, [domainEvent, ct])!;
                await task;
            }
        }
    }
}
