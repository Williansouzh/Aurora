namespace Aurora.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
