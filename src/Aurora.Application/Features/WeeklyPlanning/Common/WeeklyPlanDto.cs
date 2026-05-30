using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.WeeklyPlanning.Common;

public record WeeklyPlanDto(
    string Id,
    DateTime WeekStart,
    DateTime WeekEnd,
    string? MainFocus,
    List<string> LinkedGoalIds,
    List<string> Priorities,
    string? Notes,
    WeeklyPlanStatus Status,
    string? Review,
    int XpGenerated,
    DateTime? ClosedAt,
    DateTime CreatedAt);

public static class WeeklyPlanMappingExtensions
{
    public static WeeklyPlanDto ToDto(this WeeklyPlan p) => new(
        p.Id, p.WeekStart, p.WeekEnd, p.MainFocus, p.LinkedGoalIds,
        p.Priorities, p.Notes, p.Status, p.Review, p.XpGenerated,
        p.ClosedAt, p.CreatedAt);
}
