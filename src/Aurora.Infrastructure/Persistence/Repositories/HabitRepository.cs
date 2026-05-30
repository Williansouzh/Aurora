using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class HabitRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<Habit>(context.Habits, unitOfWork), IHabitRepository
{
    public Task<List<Habit>> GetActiveAsync(string userId) =>
        Collection.Find(x => x.UserId == userId && x.IsActive)
            .SortBy(x => x.Name)
            .ToListAsync();
}
