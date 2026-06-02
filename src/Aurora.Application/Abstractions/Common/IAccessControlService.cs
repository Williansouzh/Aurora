using Aurora.Application.Features.Access.Common;
using Aurora.Domain.Enums;

namespace Aurora.Application.Abstractions.Common;

public interface IAccessControlService
{
    Task<AccessSnapshotDto> GetSnapshotAsync(string userId, CancellationToken ct = default);
    Task<ModuleAccessDecisionDto> CanAccessModuleAsync(
        string userId,
        string moduleKey,
        string action = "read",
        CancellationToken ct = default);

    Task EnsureCanAccessModuleAsync(
        string userId,
        string moduleKey,
        string action = "read",
        CancellationToken ct = default);

    Task<bool> IsInRoleAsync(string userId, UserRole role, CancellationToken ct = default);
}
