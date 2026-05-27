using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(MongoContext context) : IRefreshTokenRepository
{
    public Task AddAsync(RefreshToken token) => context.RefreshTokens.InsertOneAsync(token);

    public Task<RefreshToken?> GetByHashAsync(string hash) =>
        context.RefreshTokens.Find(x => x.TokenHash == hash).FirstOrDefaultAsync()!;

    public async Task RevokeAsync(string id)
    {
        var update = Builders<RefreshToken>.Update
            .Set(x => x.IsRevoked, true)
            .Set(x => x.RevokedAt, DateTime.UtcNow);
        await context.RefreshTokens.UpdateOneAsync(x => x.Id == id, update);
    }

    public async Task RevokeAllByUserAsync(string userId)
    {
        var update = Builders<RefreshToken>.Update
            .Set(x => x.IsRevoked, true)
            .Set(x => x.RevokedAt, DateTime.UtcNow);
        await context.RefreshTokens.UpdateManyAsync(x => x.UserId == userId && !x.IsRevoked, update);
    }
}
