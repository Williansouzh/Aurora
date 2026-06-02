using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class LifeAreaCatalogRepository(MongoContext context) : ILifeAreaCatalogRepository
{
    public Task<List<LifeAreaCatalogItem>> GetAllAsync(CancellationToken ct = default) =>
        context.LifeAreaCatalog.Find(_ => true).ToListAsync(ct);

    public Task UpsertAsync(LifeAreaCatalogItem area, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(area.Id)) area.Id = ObjectId.GenerateNewId().ToString();
        area.UpdatedAt = DateTime.UtcNow;
        return context.LifeAreaCatalog.ReplaceOneAsync(
            x => x.Key == area.Key,
            area,
            new ReplaceOptions { IsUpsert = true },
            ct);
    }
}
