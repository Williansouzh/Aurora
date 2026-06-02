using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.WeeklyPlanning.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.WeeklyPlanning.GetCurrent;

public record GetCurrentWeeklyPlanQuery(string UserId) : IRequest<WeeklyPlanDto?>;

public class GetCurrentWeeklyPlanHandler(
    IWeeklyPlanRepository repo,
    IStudySkillRepository studySkills,
    IStudySessionRepository studySessions,
    IStudyReviewRepository studyReviews,
    IStudyPracticeTaskRepository studyPractices)
    : IRequestHandler<GetCurrentWeeklyPlanQuery, WeeklyPlanDto?>
{
    public async Task<WeeklyPlanDto?> Handle(GetCurrentWeeklyPlanQuery q, CancellationToken ct)
    {
        var plan = await repo.GetCurrentAsync(q.UserId);
        if (plan is null) return null;

        var weekStart = plan.WeekStart.Date;
        var weekEndExclusive = plan.WeekEnd.Date.AddDays(1);
        var skills = await studySkills.GetByUserAsync(q.UserId, ct);
        var active = skills.Where(x => x.Status == StudySkillStatus.Active).ToList();

        var summary = new StudyWeeklySummaryDto(
            active.Count,
            active.Sum(x => x.WeeklyTimeBudgetMinutes),
            await studySessions.SumCompletedMinutesThisWeekAsync(q.UserId, weekStart, weekEndExclusive, ct),
            await studySessions.CountCompletedAsync(q.UserId, weekStart, weekEndExclusive, ct),
            await studyReviews.CountDueAsync(q.UserId, DateTime.UtcNow.Date, ct),
            await studyReviews.CountCompletedThisWeekAsync(q.UserId, weekStart, weekEndExclusive, ct),
            await studyPractices.CountOpenAsync(q.UserId, DateTime.UtcNow.Date, ct),
            await studyPractices.CountCompletedThisWeekAsync(q.UserId, weekStart, weekEndExclusive, ct));

        return plan.ToDto(summary);
    }
}
