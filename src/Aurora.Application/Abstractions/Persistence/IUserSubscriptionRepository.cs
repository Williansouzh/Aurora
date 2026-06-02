using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IUserSubscriptionRepository
{
    Task<UserSubscription?> GetActiveByUserAsync(string userId, CancellationToken ct = default);
    Task<List<UserSubscription>> GetByUserAsync(string userId, CancellationToken ct = default);
    Task UpsertCurrentAsync(UserSubscription subscription, CancellationToken ct = default);
}
