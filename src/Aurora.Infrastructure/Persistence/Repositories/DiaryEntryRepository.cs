using System.Text.RegularExpressions;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class DiaryEntryRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<DiaryEntry>(context.DiaryEntries, unitOfWork), IDiaryEntryRepository
{
    public async Task<DiaryEntry?> GetByDateAsync(string userId, DateTime date)
    {
        var next = date.Date.AddDays(1);
        return await Collection
            .Find(x => x.UserId == userId && x.Date >= date.Date && x.Date < next)
            .FirstOrDefaultAsync();
    }

    public async Task<(List<DiaryEntry> Items, int TotalCount)> GetPagedAsync(
        string userId, string? search, int? mood, string? tag, int page, int pageSize)
    {
        var b = Builders<DiaryEntry>.Filter;
        var f = b.Eq(x => x.UserId, userId);

        if (mood.HasValue) f &= b.Eq(x => x.Mood, mood.Value);
        if (!string.IsNullOrWhiteSpace(tag)) f &= b.AnyEq(x => x.Tags, tag);
        if (!string.IsNullOrWhiteSpace(search))
            f &= b.Regex(x => x.Content, new BsonRegularExpression(Regex.Escape(search.Trim()), "i"));

        var total = (int)await Collection.CountDocumentsAsync(f);
        var items = await Collection.Find(f)
            .SortByDescending(x => x.Date)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<List<DiaryEntry>> GetRecentAsync(string userId, int limit = 7) =>
        Collection.Find(x => x.UserId == userId)
            .SortByDescending(x => x.Date)
            .Limit(limit)
            .ToListAsync();
}
