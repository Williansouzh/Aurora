using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByHashAsync(string hash);
    Task RevokeAsync(string id);
    Task RevokeAllByUserAsync(string userId);
}
