using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Retrospectives.GetWeekly;

public record GetWeeklyRetrospectiveQuery(string UserId, DateTime WeekStart) : IRequest<WeeklyRetrospectiveDto>;

public record TopHabitDto(string HabitName, int CheckIns);

public record WeeklyRetrospectiveDto(
    DateTime WeekStart,
    DateTime WeekEnd,
    int TasksCompleted,
    int HabitCheckIns,
    int HabitsWithStreak,
    double AverageMood,
    List<TopHabitDto> TopHabits,
    int XpEarned,
    string? WeeklyReview);

public class GetWeeklyRetrospectiveHandler(
    IDailyTaskRepository taskRepo,
    IHabitRepository habitRepo,
    IHabitCheckInRepository checkInRepo,
    IDiaryEntryRepository diaryRepo,
    IWeeklyPlanRepository weeklyRepo,
    IXpRepository xpRepo)
    : IRequestHandler<GetWeeklyRetrospectiveQuery, WeeklyRetrospectiveDto>
{
    public async Task<WeeklyRetrospectiveDto> Handle(GetWeeklyRetrospectiveQuery q, CancellationToken ct)
    {
        var weekStart = q.WeekStart.Date;
        var weekEnd   = weekStart.AddDays(6);

        var tHabits  = habitRepo.GetActiveAsync(q.UserId);
        var tDiary   = diaryRepo.GetRecentAsync(q.UserId, 7);
        var tPlan    = weeklyRepo.GetCurrentAsync(q.UserId);
        var tXp      = xpRepo.GetTotalForPeriodAsync(q.UserId, weekStart, weekEnd.AddDays(1));

        await Task.WhenAll(tHabits, tDiary, tPlan, tXp);

        var habits       = tHabits.Result;
        var checkInTasks = habits.Select(h => checkInRepo.GetByHabitAsync(h.Id, q.UserId, weekStart, weekEnd));
        var allCheckIns  = (await Task.WhenAll(checkInTasks)).SelectMany(x => x).ToList();
        var doneCheckIns = allCheckIns.Where(c => c.Status == HabitCheckInStatus.Done).ToList();

        var diary   = tDiary.Result.Where(d => d.Date.Date >= weekStart && d.Date.Date <= weekEnd).ToList();
        var avgMood = diary.Count > 0 ? diary.Average(d => d.Mood) : 0;

        var topHabits = doneCheckIns
            .GroupBy(c => c.HabitId)
            .Select(g => new TopHabitDto(
                habits.FirstOrDefault(h => h.Id == g.Key)?.Name ?? g.Key,
                g.Count()))
            .OrderByDescending(x => x.CheckIns)
            .Take(3)
            .ToList();

        var completedTasks = 0;
        for (var d = weekStart; d <= weekEnd; d = d.AddDays(1))
        {
            var dayTasks = await taskRepo.GetByDateAsync(q.UserId, d);
            completedTasks += dayTasks.Count(t => t.Status == DailyTaskStatus.Completed);
        }

        return new WeeklyRetrospectiveDto(
            weekStart, weekEnd,
            completedTasks,
            doneCheckIns.Count,
            habits.Count(h => h.CurrentStreak > 0),
            Math.Round(avgMood, 1),
            topHabits,
            tXp.Result,
            tPlan.Result?.Review);
    }
}
