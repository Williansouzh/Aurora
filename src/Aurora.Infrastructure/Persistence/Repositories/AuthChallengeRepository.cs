using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class AuthChallengeRepository(MongoContext context) : IAuthChallengeRepository
{
    public Task<AuthChallenge?> GetByIdAsync(string id, CancellationToken ct = default) =>
        context.AuthChallenges.Find(x => x.Id == id).FirstOrDefaultAsync(ct)!;

    public Task<AuthChallenge?> GetByTokenHashAsync(
        string tokenHash,
        string purpose,
        CancellationToken ct = default) =>
        context.AuthChallenges
            .Find(x => x.TokenHash == tokenHash && x.Purpose == purpose)
            .FirstOrDefaultAsync(ct)!;

    public Task AddAsync(AuthChallenge challenge, CancellationToken ct = default) =>
        context.AuthChallenges.InsertOneAsync(challenge, cancellationToken: ct);

    public Task UpdateAsync(AuthChallenge challenge, CancellationToken ct = default)
    {
        challenge.UpdatedAt = DateTime.UtcNow;
        return context.AuthChallenges.ReplaceOneAsync(x => x.Id == challenge.Id, challenge, cancellationToken: ct);
    }
}
