using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.Goals.Common;

public record MilestoneDto(
    string Id,
    string Title,
    bool IsRequired,
    bool IsCompleted,
    DateTime? CompletedAt);

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
    DateTime CreatedAt);

public static class GoalMappingExtensions
{
    public static MilestoneDto ToDto(this Milestone m) =>
        new(m.Id, m.Title, m.IsRequired, m.IsCompleted, m.CompletedAt);

    public static GoalDto ToDto(this Goal g) => new(
        g.Id, g.Title, g.Description, g.Area, g.Status,
        g.StartDate, g.TargetDate, g.MetricType, g.TargetValue,
        g.CurrentValue, g.Progress, g.CoverImage, g.LinkedCategoryId,
        g.Milestones.Select(m => m.ToDto()).ToList(),
        g.CreatedAt);
}
