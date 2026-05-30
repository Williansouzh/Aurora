using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.Habits.Common;

public record HabitDto(
    string Id,
    string Name,
    string? Description,
    LifeArea Area,
    HabitFrequencyType FrequencyType,
    List<int> DaysOfWeek,
    int TimesPerWeek,
    HabitDifficulty Difficulty,
    int XpReward,
    int CurrentStreak,
    int BestStreak,
    bool IsActive,
    DateTime CreatedAt);

public record HabitCheckInDto(
    string Id,
    string HabitId,
    DateTime Date,
    HabitCheckInStatus Status,
    string? Note,
    int XpGenerated,
    DateTime CreatedAt);

public static class HabitMappingExtensions
{
    public static HabitDto ToDto(this Habit h) => new(
        h.Id, h.Name, h.Description, h.Area, h.FrequencyType,
        h.DaysOfWeek, h.TimesPerWeek, h.Difficulty, h.XpReward,
        h.CurrentStreak, h.BestStreak, h.IsActive, h.CreatedAt);

    public static HabitCheckInDto ToDto(this HabitCheckIn c) => new(
        c.Id, c.HabitId, c.Date, c.Status, c.Note, c.XpGenerated, c.CreatedAt);
}
