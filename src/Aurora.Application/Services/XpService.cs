using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Services;

public class XpService(IXpRepository xpRepo, IUserRepository userRepo) : IXpService
{
    public async Task AwardAsync(string userId, XpSource source, int amount, string description, CancellationToken ct = default)
    {
        await xpRepo.AddAsync(new XpEntry
        {
            UserId = userId,
            Source = source,
            Amount = amount,
            Description = description,
            OccurredAt = DateTime.UtcNow,
        }, ct);

        var user = await userRepo.GetByIdAsync(userId);
        if (user is null) return;

        user.AddXp(amount);
        await userRepo.UpdateAsync(user);

        await CheckAchievementsAsync(userId, ct);
    }

    public async Task CheckAchievementsAsync(string userId, CancellationToken ct = default)
    {
        var user = await userRepo.GetByIdAsync(userId);
        if (user is null) return;

        var changed = false;
        foreach (var (key, minLevel) in new[] { ("level_5", 5), ("level_10", 10), ("level_20", 20), ("level_30", 30), ("level_50", 50) })
            if (user.Level >= minLevel && user.UnlockAchievement(key)) changed = true;

        if (changed) await userRepo.UpdateAsync(user);
    }
}
