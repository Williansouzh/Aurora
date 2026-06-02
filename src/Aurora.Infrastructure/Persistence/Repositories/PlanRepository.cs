using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class PlanRepository(MongoContext context) : IPlanRepository
{
    public Task<List<Plan>> GetAllAsync(CancellationToken ct = default) =>
        context.Plans.Find(_ => true).ToListAsync(ct);

    public Task<Plan?> GetByIdAsync(string id, CancellationToken ct = default) =>
        context.Plans.Find(x => x.Id == id).FirstOrDefaultAsync(ct)!;

    public Task<Plan?> GetByKeyAsync(string key, CancellationToken ct = default) =>
        context.Plans.Find(x => x.Key == key).FirstOrDefaultAsync(ct)!;

    public Task AddAsync(Plan plan, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(plan.Id)) plan.Id = ObjectId.GenerateNewId().ToString();
        return context.Plans.InsertOneAsync(plan, cancellationToken: ct);
    }

    public Task UpdateAsync(Plan plan, CancellationToken ct = default)
    {
        plan.UpdatedAt = DateTime.UtcNow;
        return context.Plans.ReplaceOneAsync(x => x.Id == plan.Id, plan, cancellationToken: ct);
    }

    public Task UpsertAsync(Plan plan, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(plan.Id)) plan.Id = ObjectId.GenerateNewId().ToString();
        plan.UpdatedAt = DateTime.UtcNow;
        return context.Plans.ReplaceOneAsync(
            x => x.Key == plan.Key,
            plan,
            new ReplaceOptions { IsUpsert = true },
            ct);
    }
}
