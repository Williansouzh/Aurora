using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class TimelineEventRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<TimelineEvent>(context.TimelineEvents, unitOfWork), ITimelineEventRepository
{
    public async Task<(List<TimelineEvent> Items, int TotalCount)> GetPagedAsync(
        string userId,
        TimelineEventType? type,
        LifeArea? area,
        DateTime? from,
        DateTime? to,
        bool favoritesOnly,
        int page,
        int pageSize)
    {
        var b = Builders<TimelineEvent>.Filter;
        var f = b.Eq(x => x.UserId, userId) & b.Eq(x => x.IsHidden, false);

        if (type.HasValue) f &= b.Eq(x => x.Type, type.Value);
        if (area.HasValue) f &= b.Eq(x => x.Area, area.Value);
        if (from.HasValue) f &= b.Gte(x => x.OccurredAt, from.Value);
        if (to.HasValue) f &= b.Lte(x => x.OccurredAt, to.Value);
        if (favoritesOnly) f &= b.Eq(x => x.IsFavorite, true);

        var total = (int)await Collection.CountDocumentsAsync(f);
        var items = await Collection.Find(f)
            .SortByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<List<TimelineEvent>> GetRecentAsync(string userId, int limit = 5) =>
        Collection.Find(x => x.UserId == userId && !x.IsHidden)
            .SortByDescending(x => x.OccurredAt)
            .Limit(limit)
            .ToListAsync();

    public Task AddFromModuleAsync(TimelineEvent evt) => AddAsync(evt);
}
