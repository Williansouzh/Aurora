using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IAuthChallengeRepository
{
    Task<AuthChallenge?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<AuthChallenge?> GetByTokenHashAsync(string tokenHash, string purpose, CancellationToken ct = default);
    Task AddAsync(AuthChallenge challenge, CancellationToken ct = default);
    Task UpdateAsync(AuthChallenge challenge, CancellationToken ct = default);
}
