using Aurora.Domain.Enums;

namespace Aurora.Application.Abstractions.Common;

public interface IXpService
{
    Task AwardAsync(string userId, XpSource source, int amount, string description, CancellationToken ct = default);
    Task CheckAchievementsAsync(string userId, CancellationToken ct = default);
}
