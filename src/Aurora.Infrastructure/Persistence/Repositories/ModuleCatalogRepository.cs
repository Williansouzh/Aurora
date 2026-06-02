using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class ModuleCatalogRepository(MongoContext context) : IModuleCatalogRepository
{
    public Task<List<ModuleCatalogItem>> GetAllAsync(CancellationToken ct = default) =>
        context.ModuleCatalog.Find(_ => true).ToListAsync(ct);

    public Task<ModuleCatalogItem?> GetByKeyAsync(string key, CancellationToken ct = default) =>
        context.ModuleCatalog.Find(x => x.Key == key).FirstOrDefaultAsync(ct)!;

    public Task UpsertAsync(ModuleCatalogItem module, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(module.Id)) module.Id = ObjectId.GenerateNewId().ToString();
        module.UpdatedAt = DateTime.UtcNow;
        return context.ModuleCatalog.ReplaceOneAsync(
            x => x.Key == module.Key,
            module,
            new ReplaceOptions { IsUpsert = true },
            ct);
    }
}
