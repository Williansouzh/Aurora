using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Access.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;

namespace Aurora.Application.Services;

public class AccessControlService(
    IUserRepository users,
    IModuleCatalogRepository modules,
    IPlanRepository plans,
    IUserSubscriptionRepository subscriptions,
    IUserModuleOverrideRepository overrides,
    ILifeAreaCatalogRepository lifeAreas) : IAccessControlService
{
    public async Task<AccessSnapshotDto> GetSnapshotAsync(string userId, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId)
            ?? throw new UnauthorizedException("Usuario nao encontrado.");

        var allModules = await modules.GetAllAsync(ct);
        var currentSubscription = await subscriptions.GetActiveByUserAsync(userId, ct);
        var plan = currentSubscription is null ? null : await plans.GetByIdAsync(currentSubscription.PlanId, ct);
        var userOverrides = await overrides.GetByUserAsync(userId, ct);
        var areas = await lifeAreas.GetAllAsync(ct);

        var moduleDtos = allModules
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.ProductName)
            .Select(module =>
            {
                var decision = Decide(user.Role, user.Status, module, plan?.ModuleKeys ?? [], userOverrides);
                var activeOverride = FindActiveOverride(module.Key, userOverrides);

                return new ModuleAccessDto(
                    module.Key,
                    module.Name,
                    module.ProductName,
                    module.Route,
                    module.Icon,
                    module.Area,
                    module.Status,
                    module.ReleaseStage,
                    module.RequiredRole,
                    module.ShowInNavigation,
                    decision.IsAllowed,
                    decision.IsReadonly,
                    decision.Reason,
                    activeOverride?.Access.ToString(),
                    module.SortOrder);
            })
            .ToList();

        return new AccessSnapshotDto(
            user.Id,
            user.Role,
            user.Status,
            plan?.Key,
            plan?.Name,
            moduleDtos,
            areas
                .OrderBy(x => x.SortOrder)
                .Select(x => new LifeAreaAccessDto(x.Key, x.Area, x.Name, x.Color, x.Icon, x.Status, x.SortOrder))
                .ToList(),
            plan?.Limits ?? [],
            DateTime.UtcNow);
    }

    public async Task<ModuleAccessDecisionDto> CanAccessModuleAsync(
        string userId,
        string moduleKey,
        string action = "read",
        CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId);
        if (user is null)
        {
            return new ModuleAccessDecisionDto(false, false, AccessDecisionReason.UserInactive, moduleKey);
        }

        var module = await modules.GetByKeyAsync(moduleKey, ct);
        if (module is null)
        {
            return new ModuleAccessDecisionDto(false, false, AccessDecisionReason.ModuleNotFound, moduleKey);
        }

        var subscription = await subscriptions.GetActiveByUserAsync(userId, ct);
        var plan = subscription is null ? null : await plans.GetByIdAsync(subscription.PlanId, ct);
        var userOverrides = await overrides.GetByUserAsync(userId, ct);

        var decision = Decide(user.Role, user.Status, module, plan?.ModuleKeys ?? [], userOverrides);
        if (decision.IsReadonly && IsWriteAction(action))
        {
            return decision with { IsAllowed = false, Reason = AccessDecisionReason.Readonly };
        }

        return decision;
    }

    public async Task EnsureCanAccessModuleAsync(
        string userId,
        string moduleKey,
        string action = "read",
        CancellationToken ct = default)
    {
        var decision = await CanAccessModuleAsync(userId, moduleKey, action, ct);
        if (!decision.IsAllowed)
        {
            throw new UnauthorizedException($"Modulo indisponivel: {moduleKey} ({decision.Reason}).");
        }
    }

    public async Task<bool> IsInRoleAsync(string userId, UserRole role, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId);
        return user is not null && user.Status == UserStatus.Active && user.Role >= role;
    }

    private static ModuleAccessDecisionDto Decide(
        UserRole userRole,
        UserStatus userStatus,
        Domain.Entities.ModuleCatalogItem module,
        List<string> planModuleKeys,
        List<Domain.Entities.UserModuleOverride> userOverrides)
    {
        if (userStatus != UserStatus.Active)
        {
            return new ModuleAccessDecisionDto(false, false, AccessDecisionReason.UserInactive, module.Key);
        }

        if (module.Status is ModuleStatus.Disabled or ModuleStatus.Archived)
        {
            return new ModuleAccessDecisionDto(false, false, AccessDecisionReason.ModuleDisabled, module.Key);
        }

        if (userRole < module.RequiredRole)
        {
            return new ModuleAccessDecisionDto(false, false, AccessDecisionReason.RoleRequired, module.Key);
        }

        if (userRole == UserRole.SuperAdmin)
        {
            return new ModuleAccessDecisionDto(true, false, AccessDecisionReason.Allowed, module.Key);
        }

        var activeOverride = FindActiveOverride(module.Key, userOverrides);
        if (activeOverride?.Access == ModuleAccess.Deny)
        {
            return new ModuleAccessDecisionDto(false, false, AccessDecisionReason.DeniedByOverride, module.Key);
        }

        if (activeOverride?.Access == ModuleAccess.Readonly)
        {
            return new ModuleAccessDecisionDto(true, true, AccessDecisionReason.Readonly, module.Key);
        }

        if (activeOverride?.Access is ModuleAccess.Allow or ModuleAccess.Beta)
        {
            return new ModuleAccessDecisionDto(true, false, AccessDecisionReason.AllowedByOverride, module.Key);
        }

        if (module.ReleaseStage == ModuleReleaseStage.Internal)
        {
            return new ModuleAccessDecisionDto(false, false, AccessDecisionReason.BetaOnly, module.Key);
        }

        if (planModuleKeys.Contains(module.Key))
        {
            return new ModuleAccessDecisionDto(true, false, AccessDecisionReason.PlanAllows, module.Key);
        }

        return new ModuleAccessDecisionDto(false, false, AccessDecisionReason.UpgradeRequired, module.Key);
    }

    private static Domain.Entities.UserModuleOverride? FindActiveOverride(
        string moduleKey,
        List<Domain.Entities.UserModuleOverride> userOverrides) =>
        userOverrides
            .Where(x => x.ModuleKey == moduleKey && (x.ExpiresAt is null || x.ExpiresAt > DateTime.UtcNow))
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefault();

    private static bool IsWriteAction(string action) =>
        action.Equals("write", StringComparison.OrdinalIgnoreCase) ||
        action.Equals("create", StringComparison.OrdinalIgnoreCase) ||
        action.Equals("update", StringComparison.OrdinalIgnoreCase) ||
        action.Equals("delete", StringComparison.OrdinalIgnoreCase);
}
