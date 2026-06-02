using Aurora.Domain.Common;

namespace Aurora.Domain.Entities;

public class AdminAuditLog : EntityBase
{
    public string ActorUserId { get; set; } = string.Empty;
    public string? TargetUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Before { get; set; }
    public string? After { get; set; }
    public string? Reason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
