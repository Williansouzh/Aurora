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

    public Task<List<DailyTask>> GetBacklogAsync(string userId) =>
        Collection.Find(x => x.UserId == userId && x.IsBacklog && x.Status == DailyTaskStatus.Pending)
            .SortBy(x => x.Priority)
            .ToListAsync();
}
