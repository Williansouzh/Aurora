using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.Goals.Common;

public record MilestoneDto(
    string Id,
    string Title,
    bool IsRequired,
    bool IsCompleted,
    DateTime? CompletedAt);

public record LinkedRefDto(string Id, string Name);

public record GoalDto(
    string Id,
    string Title,
    string? Description,
    LifeArea Area,
    GoalStatus Status,
    DateTime? StartDate,
    DateTime? TargetDate,
    GoalMetricType MetricType,
    decimal TargetValue,
    decimal CurrentValue,
    decimal Progress,
    string? CoverImage,
    string? LinkedCategoryId,
    List<MilestoneDto> Milestones,
    DateTime CreatedAt,
    List<LinkedRefDto> LinkedHabits,
    List<LinkedRefDto> LinkedSkills);

public static class GoalMappingExtensions
{
    public static MilestoneDto ToDto(this Milestone m) =>
        new(m.Id, m.Title, m.IsRequired, m.IsCompleted, m.CompletedAt);

    public static GoalDto ToDto(this Goal g) => g.ToDto(null, null);

    /// <summary>
    /// Maps a goal, optionally resolving linked ritual/skill ids to display names.
    /// When a map is null, linked refs are returned with their id as the name (caller may reload).
    /// </summary>
    public static GoalDto ToDto(
        this Goal g,
        IReadOnlyDictionary<string, string>? habitNames,
        IReadOnlyDictionary<string, string>? skillNames) => new(
        g.Id, g.Title, g.Description, g.Area, g.Status,
        g.StartDate, g.TargetDate, g.MetricType, g.TargetValue,
        g.CurrentValue, g.Progress, g.CoverImage, g.LinkedCategoryId,
        g.Milestones.Select(m => m.ToDto()).ToList(),
        g.CreatedAt,
        g.LinkedHabitIds.Select(id => new LinkedRefDto(id, habitNames?.GetValueOrDefault(id) ?? id)).ToList(),
        g.LinkedSkillIds.Select(id => new LinkedRefDto(id, skillNames?.GetValueOrDefault(id) ?? id)).ToList());
}
