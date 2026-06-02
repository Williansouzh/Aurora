using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class NotificationPreferences
{
    public bool HabitReminderEnabled { get; set; }
    public int HabitReminderHour { get; set; } = 20;
    public bool WeeklyPlanningReminderEnabled { get; set; }
    public int WeeklyPlanningReminderHour { get; set; } = 8;
}

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
    public UserRole Role { get; set; } = UserRole.User;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime? DeletedAt { get; set; }
    public string? DeletionReason { get; set; }

    // Notifications
    public NotificationPreferences Notifications { get; set; } = new();
    public DateTime? LastHabitReminderSentAt { get; set; }
    public DateTime? LastWeeklyReminderSentAt { get; set; }

    // Gamification
    public int TotalXp { get; set; }
    public int Level { get; set; } = 1;
    public List<string> Achievements { get; set; } = [];

    public void AddXp(int amount)
    {
        TotalXp += amount;
        Level = LevelCalculator.Compute(TotalXp);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool UnlockAchievement(string key)
    {
        if (Achievements.Contains(key)) return false;
        Achievements.Add(key);
        UpdatedAt = DateTime.UtcNow;
        return true;
    }
}
