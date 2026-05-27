using Aurora.Domain.Common;

namespace Aurora.Domain.Entities;

public class AuthChallenge : EntityBase
{
    public string UserId { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public string? TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int Attempts { get; set; }
    public int MaxAttempts { get; set; } = 5;
    public DateTime? ConsumedAt { get; set; }

    public bool IsConsumed => ConsumedAt.HasValue;
    public bool IsExpired(DateTime utcNow) => ExpiresAt <= utcNow;
}
