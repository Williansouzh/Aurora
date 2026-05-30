using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Goals.Common;
using Aurora.Application.Features.Habits.Common;
using Aurora.Application.Features.Timeline.Common;
using Aurora.Application.Features.Today.Common;
using Aurora.Domain.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Dashboard.GetHome;

public record GetHomeQuery(string UserId, int Month, int Year) : IRequest<HomeDto>;

public record HabitTodayDto(
    string Id, string Name, LifeArea Area,
    int CurrentStreak, int XpReward, bool CheckedInToday);

public record MoodHistoryDto(DateTime Date, int Mood);

public record HomeDto(
    int PendingTasksCount,
    int CompletedTasksCount,
    List<DailyTaskDto> TopPendingTasks,
    List<HabitTodayDto> TodayHabits,
    List<GoalDto> FeaturedGoals,
    List<TimelineEventDto> RecentEvents,
    int? TodayMood,
    List<MoodHistoryDto> MoodHistory,
    decimal TotalBalance,
    decimal MonthlyIncome,
    decimal MonthlyExpense,
    int TotalXp,
    int Level,
    string LevelName,
    int XpToNextLevel,
    List<string> Achievements);

public class GetHomeHandler(
    IDailyTaskRepository taskRepo,
    IHabitRepository habitRepo,
    IHabitCheckInRepository checkInRepo,
    IGoalRepository goalRepo,
    ITimelineEventRepository timelineRepo,
    IDiaryEntryRepository diaryRepo,
    IAccountRepository accountRepo,
    ITransactionRepository txRepo,
    IUserRepository userRepo)
    : IRequestHandler<GetHomeQuery, HomeDto>
{
    public async Task<HomeDto> Handle(GetHomeQuery q, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;

        var tTasks     = taskRepo.GetByDateAsync(q.UserId, today);
        var tHabits    = habitRepo.GetActiveAsync(q.UserId);
        var tCheckIns  = checkInRepo.GetByUserAndDateAsync(q.UserId, today);
        var tGoals     = goalRepo.GetByStatusAsync(q.UserId, GoalStatus.Active);
        var tEvents    = timelineRepo.GetRecentAsync(q.UserId, 5);
        var tDiary     = diaryRepo.GetRecentAsync(q.UserId, 7);
        var tBalance   = accountRepo.GetTotalBalanceAsync(q.UserId);
        var tIncome    = txRepo.SumAsync(q.UserId, q.Month, q.Year, TransactionType.Income, TransactionStatus.Paid);
        var tExpense   = txRepo.SumAsync(q.UserId, q.Month, q.Year, TransactionType.Expense, TransactionStatus.Paid);
        var tUser      = userRepo.GetByIdAsync(q.UserId);

        await Task.WhenAll(tTasks, tHabits, tCheckIns, tGoals, tEvents, tDiary, tBalance, tIncome, tExpense, tUser);

        var tasks     = tTasks.Result;
        var habits    = tHabits.Result;
        var checkIns  = tCheckIns.Result;
        var goals     = tGoals.Result;
        var events    = tEvents.Result;
        var diary     = tDiary.Result;
        var balance   = tBalance.Result;
        var income    = tIncome.Result;
        var expense   = tExpense.Result;
        var user      = tUser.Result;

        var checkedIds = checkIns.Select(c => c.HabitId).ToHashSet();
        var todayDiary = diary.FirstOrDefault(d => d.Date.Date == today);

        var pending = tasks.Where(t => t.Status == DailyTaskStatus.Pending).ToList();

        return new HomeDto(
            pending.Count,
            tasks.Count(t => t.Status == DailyTaskStatus.Completed),
            pending.OrderBy(t => t.Priority).Take(3).Select(t => t.ToDto()).ToList(),
            habits.Select(h => new HabitTodayDto(h.Id, h.Name, h.Area, h.CurrentStreak, h.XpReward, checkedIds.Contains(h.Id))).ToList(),
            goals.OrderByDescending(g => g.Progress).Take(3).Select(g => g.ToDto()).ToList(),
            events.Select(e => e.ToDto()).ToList(),
            todayDiary?.Mood,
            diary.OrderBy(d => d.Date).Select(d => new MoodHistoryDto(d.Date, d.Mood)).ToList(),
            balance,
            income,
            expense,
            user?.TotalXp ?? 0,
            user?.Level ?? 1,
            LevelCalculator.LevelName(user?.Level ?? 1),
            LevelCalculator.XpToNext(user?.TotalXp ?? 0),
            user?.Achievements ?? []);
    }
}
