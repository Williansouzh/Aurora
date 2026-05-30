using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class HabitCheckInRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<HabitCheckIn>(context.HabitCheckIns, unitOfWork), IHabitCheckInRepository
{
    public Task<HabitCheckIn?> GetByHabitAndDateAsync(string habitId, string userId, DateTime date)
    {
        var next = date.Date.AddDays(1);
        return Collection
            .Find(x => x.HabitId == habitId && x.UserId == userId && x.Date >= date.Date && x.Date < next)
            .FirstOrDefaultAsync()!;
    }

    public Task<List<HabitCheckIn>> GetByHabitAsync(string habitId, string userId, DateTime from, DateTime to)
    {
        var end = to.Date.AddDays(1);
        return Collection
            .Find(x => x.HabitId == habitId && x.UserId == userId && x.Date >= from.Date && x.Date < end)
            .SortBy(x => x.Date)
            .ToListAsync();
    }

    public Task<List<HabitCheckIn>> GetByUserAndDateAsync(string userId, DateTime date)
    {
        var next = date.Date.AddDays(1);
        return Collection
            .Find(x => x.UserId == userId && x.Date >= date.Date && x.Date < next)
            .ToListAsync();
    }
}
