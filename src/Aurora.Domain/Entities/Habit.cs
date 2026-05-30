using Aurora.Domain.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;

namespace Aurora.Domain.Entities;

public class Habit : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LifeArea Area { get; set; } = LifeArea.Health;
    public HabitFrequencyType FrequencyType { get; set; } = HabitFrequencyType.Daily;
    public List<int> DaysOfWeek { get; set; } = [];
    public int TimesPerWeek { get; set; } = 1;
    public HabitDifficulty Difficulty { get; set; } = HabitDifficulty.Medium;
    public int XpReward { get; set; }
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }
    public bool IsActive { get; set; } = true;

    public void Pause()
    {
        if (!IsActive) throw new DomainException("Hábito já está pausado.");
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resume()
    {
        if (IsActive) throw new DomainException("Hábito já está ativo.");
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStreak(int current)
    {
        CurrentStreak = current;
        if (current > BestStreak) BestStreak = current;
        UpdatedAt = DateTime.UtcNow;
    }

    public static int XpForDifficulty(HabitDifficulty difficulty) => difficulty switch
    {
        HabitDifficulty.Easy => 5,
        HabitDifficulty.Medium => 10,
        HabitDifficulty.Hard => 20,
        _ => 5,
    };
}
