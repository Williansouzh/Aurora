using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.WeeklyPlanning.Common;

public record StudyWeeklySummaryDto(
    int ActivePriorities,
    int PlannedStudyMinutes,
    int CompletedStudyMinutes,
    int CompletedSessions,
    int DueReviews,
    int CompletedReviews,
    int OpenPracticeTasks,
    int CompletedPracticeTasks);

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
    StudyWeeklySummaryDto? Studies,
    DateTime CreatedAt);

public static class WeeklyPlanMappingExtensions
{
    public static WeeklyPlanDto ToDto(this WeeklyPlan p, StudyWeeklySummaryDto? studies = null) => new(
        p.Id, p.WeekStart, p.WeekEnd, p.MainFocus, p.LinkedGoalIds,
        p.Priorities, p.Notes, p.Status, p.Review, p.XpGenerated,
        p.ClosedAt, studies, p.CreatedAt);
}
