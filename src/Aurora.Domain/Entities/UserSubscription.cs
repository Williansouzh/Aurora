using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class UserSubscription : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndsAt { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
