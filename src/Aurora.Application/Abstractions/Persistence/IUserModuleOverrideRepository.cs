using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IUserModuleOverrideRepository
{
    Task<List<UserModuleOverride>> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<UserModuleOverride?> GetByUserAndModuleAsync(string userId, string moduleKey, CancellationToken ct = default);
    Task UpsertAsync(UserModuleOverride moduleOverride, CancellationToken ct = default);
    Task DeleteAsync(string userId, string moduleKey, CancellationToken ct = default);
}
