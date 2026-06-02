using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Studies.GetDashboard;

public record GetStudiesDashboardQuery(string UserId) : IRequest<StudyDashboardDto>;

public class GetStudiesDashboardHandler(
    IStudySkillRepository skills,
    IStudySessionRepository sessions,
    IStudyReviewRepository reviews,
    IStudyPracticeTaskRepository practices)
    : IRequestHandler<GetStudiesDashboardQuery, StudyDashboardDto>
{
    public async Task<StudyDashboardDto> Handle(GetStudiesDashboardQuery query, CancellationToken ct)
    {
        var all = await skills.GetByUserAsync(query.UserId, ct);
        var recentSessions = await sessions.GetRecentAsync(query.UserId, 5, ct);
        var practiceItems = await practices.GetRecentAsync(query.UserId, 5, ct);
        var skillTitles = all.ToDictionary(x => x.Id, x => x.Title);
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-((int)today.DayOfWeek == 0 ? 6 : (int)today.DayOfWeek - 1));
        var weekEnd = weekStart.AddDays(7);
        var dueReviewItems = await reviews.GetDueAsync(query.UserId, today, 5, ct);
        var active = all
            .Where(x => x.Status == StudySkillStatus.Active)
            .OrderBy(x => x.PriorityRank ?? int.MaxValue)
            .ThenByDescending(x => x.PriorityScore)
            .ToList();

        var recommended = all
            .Where(x => x.Status is StudySkillStatus.Backlog or StudySkillStatus.Paused)
            .OrderByDescending(x => x.PriorityScore)
            .ThenBy(x => x.CreatedAt)
            .Take(Math.Max(0, 3 - active.Count))
            .ToList();

        return new StudyDashboardDto(
            all.Count,
            active.Count,
            all.Count(x => x.Status == StudySkillStatus.Backlog),
            active.Sum(x => x.WeeklyTimeBudgetMinutes),
            await sessions.SumCompletedMinutesThisWeekAsync(query.UserId, weekStart, weekEnd, ct),
            await reviews.CountDueAsync(query.UserId, today, ct),
            await reviews.CountCompletedThisWeekAsync(query.UserId, weekStart, weekEnd, ct),
            await practices.CountOpenAsync(query.UserId, today, ct),
            await practices.CountCompletedThisWeekAsync(query.UserId, weekStart, weekEnd, ct),
            await practices.AverageDifficultyCompletedThisWeekAsync(query.UserId, weekStart, weekEnd, ct),
            active.Select(x => x.ToDto()).ToList(),
            recommended.Select(x => x.ToDto()).ToList(),
            recentSessions.Select(x => x.ToDto(skillTitles.GetValueOrDefault(x.SkillId, "Estudo"))).ToList(),
            dueReviewItems.Select(x => x.ToDto(skillTitles.GetValueOrDefault(x.SkillId, "Estudo"))).ToList(),
            practiceItems.Select(x => x.ToDto(skillTitles.GetValueOrDefault(x.SkillId, "Estudo"))).ToList());
    }
}
