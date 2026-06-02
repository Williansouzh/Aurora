using System.Text.Json;
using Aurora.API.Authorization;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Access.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, RequireModule(ModuleKeys.Admin), Route("api/admin")]
public class AdminController(
    IUserContext currentUser,
    IUserRepository users,
    IPlanRepository plans,
    IModuleCatalogRepository modules,
    IUserSubscriptionRepository subscriptions,
    IUserModuleOverrideRepository overrides,
    ILifeAreaCatalogRepository lifeAreas,
    IAdminAuditLogRepository audit,
    IAccessControlService access) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> Users([FromQuery] string? search, CancellationToken ct)
    {
        var allUsers = await users.GetAllAsync();
        var allPlans = await plans.GetAllAsync(ct);
        var result = new List<AdminUserDto>();

        foreach (var user in allUsers
            .Where(x => string.IsNullOrWhiteSpace(search) ||
                x.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Name))
        {
            var sub = await subscriptions.GetActiveByUserAsync(user.Id, ct);
            var plan = sub is null ? null : allPlans.FirstOrDefault(x => x.Id == sub.PlanId);
            var userOverrides = await overrides.GetByUserAsync(user.Id, ct);

            result.Add(new AdminUserDto(
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.Status,
                plan?.Key,
                plan?.Name,
                userOverrides.Count,
                user.CreatedAt,
                user.UpdatedAt));
        }

        return Ok(new ApiResponse<List<AdminUserDto>>(true, result));
    }

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> UserDetail(string userId, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(userId);
        if (user is null) return NotFound(new ApiResponse<string>(false, "Usuario nao encontrado."));

        var snapshot = await access.GetSnapshotAsync(userId, ct);
        var sub = await subscriptions.GetActiveByUserAsync(userId, ct);
        var userOverrides = await overrides.GetByUserAsync(userId, ct);

        return Ok(new ApiResponse<AdminUserDetailDto>(true, new(
            new AdminUserDto(
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.Status,
                snapshot.PlanKey,
                snapshot.PlanName,
                userOverrides.Count,
                user.CreatedAt,
                user.UpdatedAt),
            sub?.Status,
            snapshot.Modules,
            userOverrides.Select(x => new AdminModuleOverrideDto(
                x.ModuleKey,
                x.Access,
                x.Reason,
                x.ExpiresAt,
                x.CreatedByUserId)).ToList())));
    }

    [HttpPut("users/{userId}/role")]
    public async Task<IActionResult> UpdateRole(string userId, UpdateUserRoleRequest req, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(userId);
        if (user is null) return NotFound(new ApiResponse<string>(false, "Usuario nao encontrado."));

        var before = new { user.Role, user.Status };
        user.Role = req.Role;
        user.Status = req.Status;
        await users.UpdateAsync(user);
        await AddAuditAsync("user.role.updated", "User", user.Id, before, new { user.Role, user.Status }, req.Reason, ct);

        return Ok(new ApiResponse<string>(true, "updated"));
    }

    [HttpPut("users/{userId}/plan")]
    public async Task<IActionResult> UpdatePlan(string userId, UpdateUserPlanRequest req, CancellationToken ct)
    {
        var plan = await plans.GetByKeyAsync(req.PlanKey, ct);
        if (plan is null) return NotFound(new ApiResponse<string>(false, "Plano nao encontrado."));

        var before = await subscriptions.GetActiveByUserAsync(userId, ct);
        await subscriptions.UpsertCurrentAsync(new UserSubscription
        {
            UserId = userId,
            PlanId = plan.Id,
            Status = req.Status,
            StartedAt = DateTime.UtcNow
        }, ct);

        await AddAuditAsync("user.plan.updated", "UserSubscription", userId, before, new { req.PlanKey, req.Status }, req.Reason, ct);
        return Ok(new ApiResponse<string>(true, "updated"));
    }

    [HttpPut("users/{userId}/modules/{moduleKey}")]
    public async Task<IActionResult> UpsertModuleOverride(
        string userId,
        string moduleKey,
        UpsertModuleOverrideRequest req,
        CancellationToken ct)
    {
        var module = await modules.GetByKeyAsync(moduleKey, ct);
        if (module is null) return NotFound(new ApiResponse<string>(false, "Modulo nao encontrado."));

        var before = await overrides.GetByUserAndModuleAsync(userId, moduleKey, ct);
        var item = new UserModuleOverride
        {
            UserId = userId,
            ModuleKey = moduleKey,
            Access = req.Access,
            Reason = req.Reason,
            ExpiresAt = req.ExpiresAt,
            CreatedByUserId = currentUser.UserId
        };

        await overrides.UpsertAsync(item, ct);
        await AddAuditAsync("user.module.override.upserted", "UserModuleOverride", userId, before, item, req.Reason, ct);

        return Ok(new ApiResponse<string>(true, "updated"));
    }

    [HttpDelete("users/{userId}/modules/{moduleKey}")]
    public async Task<IActionResult> DeleteModuleOverride(string userId, string moduleKey, CancellationToken ct)
    {
        var before = await overrides.GetByUserAndModuleAsync(userId, moduleKey, ct);
        await overrides.DeleteAsync(userId, moduleKey, ct);
        await AddAuditAsync("user.module.override.deleted", "UserModuleOverride", userId, before, null, null, ct);
        return Ok(new ApiResponse<string>(true, "deleted"));
    }

    [HttpGet("plans")]
    public async Task<IActionResult> Plans(CancellationToken ct) =>
        Ok(new ApiResponse<List<Plan>>(true, (await plans.GetAllAsync(ct)).OrderBy(x => x.Name).ToList()));

    [HttpPut("plans/{planKey}/modules")]
    public async Task<IActionResult> UpdatePlanModules(string planKey, UpdatePlanModulesRequest req, CancellationToken ct)
    {
        var plan = await plans.GetByKeyAsync(planKey, ct);
        if (plan is null) return NotFound(new ApiResponse<string>(false, "Plano nao encontrado."));

        var before = plan.ModuleKeys.ToList();
        plan.ModuleKeys = req.ModuleKeys.Distinct().OrderBy(x => x).ToList();
        await plans.UpdateAsync(plan, ct);
        await AddAuditAsync("plan.modules.updated", "Plan", plan.Id, before, plan.ModuleKeys, req.Reason, ct);
        return Ok(new ApiResponse<Plan>(true, plan));
    }

    [HttpGet("modules")]
    public async Task<IActionResult> Modules(CancellationToken ct) =>
        Ok(new ApiResponse<List<ModuleCatalogItem>>(true, (await modules.GetAllAsync(ct)).OrderBy(x => x.SortOrder).ToList()));

    [HttpPut("modules/{moduleKey}")]
    public async Task<IActionResult> UpdateModule(string moduleKey, UpdateModuleRequest req, CancellationToken ct)
    {
        var module = await modules.GetByKeyAsync(moduleKey, ct);
        if (module is null) return NotFound(new ApiResponse<string>(false, "Modulo nao encontrado."));

        var before = new { module.Status, module.ReleaseStage, module.ShowInNavigation };
        module.Status = req.Status;
        module.ReleaseStage = req.ReleaseStage;
        module.ShowInNavigation = req.ShowInNavigation;
        await modules.UpsertAsync(module, ct);
        await AddAuditAsync("module.updated", "ModuleCatalogItem", module.Id, before,
            new { module.Status, module.ReleaseStage, module.ShowInNavigation }, req.Reason, ct);

        return Ok(new ApiResponse<ModuleCatalogItem>(true, module));
    }

    [HttpGet("life-areas")]
    public async Task<IActionResult> LifeAreas(CancellationToken ct) =>
        Ok(new ApiResponse<List<LifeAreaAccessDto>>(true,
            (await lifeAreas.GetAllAsync(ct))
            .OrderBy(x => x.SortOrder)
            .Select(x => new LifeAreaAccessDto(x.Key, x.Area, x.Name, x.Color, x.Icon, x.Status, x.SortOrder))
            .ToList()));

    [HttpGet("audit-logs")]
    public async Task<IActionResult> AuditLogs([FromQuery] int limit = 100, CancellationToken ct = default) =>
        Ok(new ApiResponse<List<AdminAuditLog>>(true, await audit.GetRecentAsync(Math.Clamp(limit, 1, 200), ct)));

    private async Task AddAuditAsync(
        string action,
        string entityType,
        string? entityId,
        object? before,
        object? after,
        string? reason,
        CancellationToken ct)
    {
        await audit.AddAsync(new AdminAuditLog
        {
            ActorUserId = currentUser.UserId,
            TargetUserId = entityType.StartsWith("User", StringComparison.OrdinalIgnoreCase) ? entityId : null,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Before = before is null ? null : JsonSerializer.Serialize(before),
            After = after is null ? null : JsonSerializer.Serialize(after),
            Reason = reason,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            OccurredAt = DateTime.UtcNow
        }, ct);
    }
}

public record AdminUserDto(
    string UserId,
    string Name,
    string Email,
    UserRole Role,
    UserStatus Status,
    string? PlanKey,
    string? PlanName,
    int OverrideCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record AdminUserDetailDto(
    AdminUserDto User,
    SubscriptionStatus? SubscriptionStatus,
    List<ModuleAccessDto> Modules,
    List<AdminModuleOverrideDto> Overrides);

public record AdminModuleOverrideDto(
    string ModuleKey,
    ModuleAccess Access,
    string? Reason,
    DateTime? ExpiresAt,
    string? CreatedByUserId);

public record UpdateUserRoleRequest(UserRole Role, UserStatus Status, string? Reason);
public record UpdateUserPlanRequest(string PlanKey, SubscriptionStatus Status, string? Reason);
public record UpsertModuleOverrideRequest(ModuleAccess Access, string? Reason, DateTime? ExpiresAt);
public record UpdatePlanModulesRequest(List<string> ModuleKeys, string? Reason);
public record UpdateModuleRequest(
    ModuleStatus Status,
    ModuleReleaseStage ReleaseStage,
    bool ShowInNavigation,
    string? Reason);
