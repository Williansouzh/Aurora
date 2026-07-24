using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Access.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;

namespace Aurora.Application.Services;

public class AccessControlService(
    IUserRepository users,
    IModuleCatalogRepository modules,
    IPlanRepository plans,
    IUserSubscriptionRepository subscriptions,
    IUserModuleOverrideRepository overrides,
    ILifeAreaCatalogRepository lifeAreas,
    ICacheService cache) : IAccessControlService
{
    // Access checks run on nearly every module-gated request. The per-user context and the module
    // catalog are cached for a short window so the common path serves from Redis instead of issuing
    // ~5 Mongo queries; the TTL bounds staleness for catalog/plan changes and the admin write paths
    // invalidate explicitly for immediate effect.
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    public async Task<AccessSnapshotDto> GetSnapshotAsync(string userId, CancellationToken ct = default)
    {
        var context = await GetContextAsync(userId, ct)
            ?? throw new UnauthorizedException("Usuario nao encontrado.");

        var allModules = await GetModulesAsync(ct);
        var areas = await lifeAreas.GetAllAsync(ct);

        var moduleDtos = allModules
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.ProductName)
            .Select(module =>
            {
                var decision = Decide(context.Role, context.Status, module, context.PlanModuleKeys, context.Overrides);
                var activeOverride = FindActiveOverride(module.Key, context.Overrides);

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
            userId,
            context.Role,
            context.Status,
            context.PlanKey,
            context.PlanName,
            moduleDtos,
            areas
                .OrderBy(x => x.SortOrder)
                .Select(x => new LifeAreaAccessDto(x.Key, x.Area, x.Name, x.Color, x.Icon, x.Status, x.SortOrder))
                .ToList(),
            context.PlanLimits,
            DateTime.UtcNow);
    }

    public async Task<ModuleAccessDecisionDto> CanAccessModuleAsync(
        string userId,
        string moduleKey,
        string action = "read",
        CancellationToken ct = default)
    {
        var context = await GetContextAsync(userId, ct);
        if (context is null)
        {
            return new ModuleAccessDecisionDto(false, false, AccessDecisionReason.UserInactive, moduleKey);
        }

        var module = (await GetModulesAsync(ct)).FirstOrDefault(x => x.Key == moduleKey);
        if (module is null)
        {
            return new ModuleAccessDecisionDto(false, false, AccessDecisionReason.ModuleNotFound, moduleKey);
        }

        var decision = Decide(context.Role, context.Status, module, context.PlanModuleKeys, context.Overrides);
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
        var context = await GetContextAsync(userId, ct);
        return context is not null && context.Status == UserStatus.Active && context.Role >= role;
    }

    public Task InvalidateUserAsync(string userId, CancellationToken ct = default) =>
        cache.RemoveAsync(CacheKeys.AccessContext(userId), ct);

    public Task InvalidateModuleCatalogAsync(CancellationToken ct = default) =>
        cache.RemoveAsync(CacheKeys.AccessModuleCatalog(), ct);

    private async Task<List<ModuleCatalogItem>> GetModulesAsync(CancellationToken ct)
    {
        var cached = await cache.GetAsync<List<ModuleCatalogItem>>(CacheKeys.AccessModuleCatalog(), ct);
        if (cached is not null)
        {
            return cached;
        }

        var all = await modules.GetAllAsync(ct);
        await cache.SetAsync(CacheKeys.AccessModuleCatalog(), all, CacheTtl, ct);
        return all;
    }

    private async Task<CachedAccessContext?> GetContextAsync(string userId, CancellationToken ct)
    {
        var cached = await cache.GetAsync<CachedAccessContext>(CacheKeys.AccessContext(userId), ct);
        if (cached is not null)
        {
            return cached;
        }

        var user = await users.GetByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var subscription = await subscriptions.GetActiveByUserAsync(userId, ct);
        var plan = subscription is null ? null : await plans.GetByIdAsync(subscription.PlanId, ct);
        var userOverrides = await overrides.GetByUserAsync(userId, ct);

        var context = new CachedAccessContext(
            user.Role,
            user.Status,
            plan?.Key,
            plan?.Name,
            plan?.ModuleKeys ?? [],
            plan?.Limits ?? [],
            userOverrides
                .Select(o => new CachedOverride(o.ModuleKey, o.Access, o.ExpiresAt, o.UpdatedAt))
                .ToList());

        await cache.SetAsync(CacheKeys.AccessContext(userId), context, CacheTtl, ct);
        return context;
    }

    private static ModuleAccessDecisionDto Decide(
        UserRole userRole,
        UserStatus userStatus,
        ModuleCatalogItem module,
        List<string> planModuleKeys,
        List<CachedOverride> userOverrides)
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

    private static CachedOverride? FindActiveOverride(string moduleKey, List<CachedOverride> userOverrides) =>
        userOverrides
            .Where(x => x.ModuleKey == moduleKey && (x.ExpiresAt is null || x.ExpiresAt > DateTime.UtcNow))
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefault();

    private static bool IsWriteAction(string action) =>
        action.Equals("write", StringComparison.OrdinalIgnoreCase) ||
        action.Equals("create", StringComparison.OrdinalIgnoreCase) ||
        action.Equals("update", StringComparison.OrdinalIgnoreCase) ||
        action.Equals("delete", StringComparison.OrdinalIgnoreCase);

    private sealed record CachedAccessContext(
        UserRole Role,
        UserStatus Status,
        string? PlanKey,
        string? PlanName,
        List<string> PlanModuleKeys,
        Dictionary<string, int> PlanLimits,
        List<CachedOverride> Overrides);

    private sealed record CachedOverride(
        string ModuleKey,
        ModuleAccess Access,
        DateTime? ExpiresAt,
        DateTime UpdatedAt);
}
