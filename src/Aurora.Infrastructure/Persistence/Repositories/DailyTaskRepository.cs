using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class DailyTaskRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<DailyTask>(context.DailyTasks, unitOfWork), IDailyTaskRepository
{
    public Task<List<DailyTask>> GetByDateAsync(string userId, DateTime date)
    {
        var next = date.Date.AddDays(1);
        return Collection
            .Find(x => x.UserId == userId && !x.IsBacklog && x.Date >= date.Date && x.Date < next)
            .SortBy(x => x.Priority)
            .ToListAsync();
    }

    public Task<List<DailyTask>> GetOverdueAsync(string userId, DateTime before)
    {
        return Collection
            .Find(x => x.UserId == userId
                    && !x.IsBacklog
                    && x.Date < before.Date
                    && x.Status == DailyTaskStatus.Pending)
            .SortByDescending(x => x.Date)
            .ToListAsync();
    }

    public async Task<(List<DailyTask> Items, int TotalCount)> GetBacklogPagedAsync(string userId, int page, int pageSize)
    {
        var filter = Builders<DailyTask>.Filter.Where(
            x => x.UserId == userId && x.IsBacklog && x.Status == DailyTaskStatus.Pending);
        var total = (int)await Collection.CountDocumentsAsync(filter);
        var items = await Collection
            .Find(filter)
            .SortBy(x => x.Priority)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<DailyTask?> GetBySourceAsync(string userId, string sourceModule, string sourceId, CancellationToken ct = default) =>
        await Collection.Find(x => x.UserId == userId && x.SourceModule == sourceModule && x.SourceId == sourceId)
            .FirstOrDefaultAsync(ct);
}
