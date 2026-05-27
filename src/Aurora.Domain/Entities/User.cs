using Aurora.Domain.Common;

namespace Aurora.Domain.Entities;

public class User : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EmailHash { get; set; } = string.Empty;
    public string EmailEncrypted { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; }
    public DateTime? EmailConfirmedAt { get; set; }
    public bool IsMfaEnabled { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public string? DeletionReason { get; set; }
}
