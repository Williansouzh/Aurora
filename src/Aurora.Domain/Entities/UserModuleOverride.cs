using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class UserModuleOverride : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string ModuleKey { get; set; } = string.Empty;
    public ModuleAccess Access { get; set; } = ModuleAccess.Allow;
    public string? Reason { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? CreatedByUserId { get; set; }
}
