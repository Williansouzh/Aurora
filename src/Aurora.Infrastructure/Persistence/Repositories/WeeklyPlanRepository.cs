using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class WeeklyPlanRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<WeeklyPlan>(context.WeeklyPlans, unitOfWork), IWeeklyPlanRepository
{
    public async Task<WeeklyPlan?> GetCurrentAsync(string userId)
    {
        var today = DateTime.UtcNow.Date;
        return await Collection
            .Find(x => x.UserId == userId && x.WeekStart <= today && x.WeekEnd >= today)
            .FirstOrDefaultAsync();
    }

    public Task<List<WeeklyPlan>> GetRecentAsync(string userId, int limit = 10) =>
        Collection.Find(x => x.UserId == userId)
            .SortByDescending(x => x.WeekStart)
            .Limit(limit)
            .ToListAsync();

    public Task<bool> ExistsForWeekAsync(string userId, DateTime weekStart) =>
        Collection.Find(x => x.UserId == userId && x.WeekStart == weekStart.Date).AnyAsync();
}
