using Aurora.Domain.Common;

namespace Aurora.Domain.Events;

public sealed record TransactionPaidEvent(
    string TransactionId,
    string UserId,
    string AccountId,
    decimal Amount,
    DateTime OccurredAt) : IDomainEvent;
