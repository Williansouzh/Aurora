using Aurora.Domain.Common;

namespace Aurora.Domain.Entities;

public class AuditEntry : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? IpHash { get; set; }
    public string? Metadata { get; set; }
}
