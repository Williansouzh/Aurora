namespace Aurora.Domain.Enums;

public enum AccessDecisionReason
{
    Allowed = 1,
    Readonly = 2,
    UserInactive = 3,
    ModuleNotFound = 4,
    ModuleDisabled = 5,
    RoleRequired = 6,
    DeniedByOverride = 7,
    AllowedByOverride = 8,
    PlanAllows = 9,
    UpgradeRequired = 10,
    BetaOnly = 11,
}
