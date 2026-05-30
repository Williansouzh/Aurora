using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class GoalRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<Goal>(context.Goals, unitOfWork), IGoalRepository
{
    public async Task<List<Goal>> GetByStatusAsync(string userId, GoalStatus? status)
    {
        var filter = status.HasValue
            ? Builders<Goal>.Filter.Eq(x => x.UserId, userId) & Builders<Goal>.Filter.Eq(x => x.Status, status.Value)
            : Builders<Goal>.Filter.Eq(x => x.UserId, userId);

        return await Collection.Find(filter).SortByDescending(x => x.CreatedAt).ToListAsync();
    }
}
