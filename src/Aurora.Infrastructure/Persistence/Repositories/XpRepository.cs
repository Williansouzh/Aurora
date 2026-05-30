using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class XpRepository(MongoContext context) : IXpRepository
{
    private readonly IMongoCollection<XpEntry> _col = context.XpEntries;

    public Task AddAsync(XpEntry entry, CancellationToken ct = default) =>
        _col.InsertOneAsync(entry, cancellationToken: ct);

    public Task<List<XpEntry>> GetRecentAsync(string userId, int limit = 20) =>
        _col.Find(x => x.UserId == userId)
            .SortByDescending(x => x.OccurredAt)
            .Limit(limit)
            .ToListAsync();

    public async Task<int> GetTotalForPeriodAsync(string userId, DateTime from, DateTime to)
    {
        var result = await _col.Aggregate()
            .Match(x => x.UserId == userId && x.OccurredAt >= from && x.OccurredAt <= to)
            .Group(_ => 1, g => new { Total = g.Sum(x => x.Amount) })
            .FirstOrDefaultAsync();
        return result?.Total ?? 0;
    }

    public Task<bool> ExistsAsync(string userId, XpSource source, DateTime from, DateTime to) =>
        _col.Find(x => x.UserId == userId && x.Source == source && x.OccurredAt >= from && x.OccurredAt <= to)
            .AnyAsync();
}
