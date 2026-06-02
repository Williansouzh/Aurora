using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class UserModuleOverrideRepository(MongoContext context) : IUserModuleOverrideRepository
{
    public Task<List<UserModuleOverride>> GetByUserAsync(string userId, CancellationToken ct = default) =>
        context.UserModuleOverrides.Find(x => x.UserId == userId).ToListAsync(ct);

    public Task<UserModuleOverride?> GetByUserAndModuleAsync(string userId, string moduleKey, CancellationToken ct = default) =>
        context.UserModuleOverrides.Find(x => x.UserId == userId && x.ModuleKey == moduleKey).FirstOrDefaultAsync(ct)!;

    public Task UpsertAsync(UserModuleOverride moduleOverride, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(moduleOverride.Id)) moduleOverride.Id = ObjectId.GenerateNewId().ToString();
        moduleOverride.UpdatedAt = DateTime.UtcNow;
        return context.UserModuleOverrides.ReplaceOneAsync(
            x => x.UserId == moduleOverride.UserId && x.ModuleKey == moduleOverride.ModuleKey,
            moduleOverride,
            new ReplaceOptions { IsUpsert = true },
            ct);
    }

    public Task DeleteAsync(string userId, string moduleKey, CancellationToken ct = default) =>
        context.UserModuleOverrides.DeleteOneAsync(x => x.UserId == userId && x.ModuleKey == moduleKey, ct);
}
