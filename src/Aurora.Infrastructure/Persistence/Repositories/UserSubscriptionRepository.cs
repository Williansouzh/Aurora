using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class UserSubscriptionRepository(MongoContext context) : IUserSubscriptionRepository
{
    private static readonly SubscriptionStatus[] ActiveStatuses =
    [
        SubscriptionStatus.Active,
        SubscriptionStatus.Trial,
        SubscriptionStatus.Internal
    ];

    public Task<UserSubscription?> GetActiveByUserAsync(string userId, CancellationToken ct = default) =>
        context.UserSubscriptions
            .Find(x => x.UserId == userId && ActiveStatuses.Contains(x.Status))
            .SortByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(ct)!;

    public Task<List<UserSubscription>> GetActiveByUsersAsync(IEnumerable<string> userIds, CancellationToken ct = default)
    {
        var ids = userIds.Distinct().ToArray();
        return context.UserSubscriptions
            .Find(x => ids.Contains(x.UserId) && ActiveStatuses.Contains(x.Status))
            .ToListAsync(ct);
    }

    public Task<List<UserSubscription>> GetByUserAsync(string userId, CancellationToken ct = default) =>
        context.UserSubscriptions.Find(x => x.UserId == userId).ToListAsync(ct);

    public async Task UpsertCurrentAsync(UserSubscription subscription, CancellationToken ct = default)
    {
        var existing = await GetActiveByUserAsync(subscription.UserId, ct);
        if (existing is not null)
        {
            subscription.Id = existing.Id;
            subscription.CreatedAt = existing.CreatedAt;
        }

        if (string.IsNullOrWhiteSpace(subscription.Id)) subscription.Id = ObjectId.GenerateNewId().ToString();
        subscription.UpdatedAt = DateTime.UtcNow;
        await context.UserSubscriptions.ReplaceOneAsync(
            x => x.UserId == subscription.UserId && ActiveStatuses.Contains(x.Status),
            subscription,
            new ReplaceOptions { IsUpsert = true },
            ct);
    }
}
