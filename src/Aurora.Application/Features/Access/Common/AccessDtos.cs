using Aurora.Domain.Enums;

namespace Aurora.Application.Features.Access.Common;

public record AccessSnapshotDto(
    string UserId,
    UserRole Role,
    UserStatus Status,
    string? PlanKey,
    string? PlanName,
    List<ModuleAccessDto> Modules,
    List<LifeAreaAccessDto> LifeAreas,
    Dictionary<string, int> Limits,
    DateTime GeneratedAt);

public record ModuleAccessDto(
    string Key,
    string Name,
    string ProductName,
    string Route,
    string Icon,
    LifeArea? Area,
    ModuleStatus Status,
    ModuleReleaseStage ReleaseStage,
    UserRole RequiredRole,
    bool ShowInNavigation,
    bool IsAllowed,
    bool IsReadonly,
    AccessDecisionReason Reason,
    string? OverrideAccess,
    int SortOrder);

public record LifeAreaAccessDto(
    string Key,
    LifeArea Area,
    string Name,
    string Color,
    string Icon,
    ModuleStatus Status,
    int SortOrder);

public record ModuleAccessDecisionDto(
    bool IsAllowed,
    bool IsReadonly,
    AccessDecisionReason Reason,
    string ModuleKey);
