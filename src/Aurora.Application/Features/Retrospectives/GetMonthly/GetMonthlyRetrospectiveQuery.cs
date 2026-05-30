using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Retrospectives.GetMonthly;

public record GetMonthlyRetrospectiveQuery(string UserId, int Month, int Year) : IRequest<MonthlyRetrospectiveDto>;

public record MonthlyRetrospectiveDto(
    int Month,
    int Year,
    int TasksCompleted,
    int HabitCheckIns,
    int NewGoalsCreated,
    int GoalsCompleted,
    double AverageMood,
    int MoodEntriesCount,
    int DiaryEntriesCount,
    decimal MonthlyIncome,
    decimal MonthlyExpense,
    decimal MonthlySavings,
    int XpEarned,
    List<string> UnlockedAchievements);

public class GetMonthlyRetrospectiveHandler(
    IHabitRepository habitRepo,
    IHabitCheckInRepository checkInRepo,
    IGoalRepository goalRepo,
    IDiaryEntryRepository diaryRepo,
    ITransactionRepository txRepo,
    IXpRepository xpRepo)
    : IRequestHandler<GetMonthlyRetrospectiveQuery, MonthlyRetrospectiveDto>
{
    public async Task<MonthlyRetrospectiveDto> Handle(GetMonthlyRetrospectiveQuery q, CancellationToken ct)
    {
        var from = new DateTime(q.Year, q.Month, 1);
        var to   = from.AddMonths(1).AddDays(-1);

        var tHabits  = habitRepo.GetByUserAsync(q.UserId, ct);
        var tGoals   = goalRepo.GetByStatusAsync(q.UserId, null);
        var tDiary   = diaryRepo.GetPagedAsync(q.UserId, null, null, null, 1, 100);
        var tIncome  = txRepo.SumAsync(q.UserId, q.Month, q.Year, TransactionType.Income, TransactionStatus.Paid);
        var tExpense = txRepo.SumAsync(q.UserId, q.Month, q.Year, TransactionType.Expense, TransactionStatus.Paid);
        var tXp      = xpRepo.GetTotalForPeriodAsync(q.UserId, from, to.AddDays(1));

        await Task.WhenAll(tHabits, tGoals, tDiary, tIncome, tExpense, tXp);

        var habits = tHabits.Result;
        var checkInTasks = habits.Select(h => checkInRepo.GetByHabitAsync(h.Id, q.UserId, from, to));
        var allCheckIns = (await Task.WhenAll(checkInTasks)).SelectMany(x => x)
            .Where(c => c.Status == HabitCheckInStatus.Done).ToList();

        var monthDiary = tDiary.Result.Items
            .Where(d => d.Date.Year == q.Year && d.Date.Month == q.Month).ToList();
        var avgMood = monthDiary.Count > 0 ? monthDiary.Average(d => d.Mood) : 0;

        var goals = tGoals.Result;
        var newGoals = goals.Count(g => g.CreatedAt.Year == q.Year && g.CreatedAt.Month == q.Month);
        var completedGoals = goals.Count(g =>
            g.Status == GoalStatus.Completed && g.UpdatedAt.Year == q.Year && g.UpdatedAt.Month == q.Month);

        var income = tIncome.Result;
        var expense = tExpense.Result;

        return new MonthlyRetrospectiveDto(
            q.Month, q.Year,
            0, // task count is expensive to compute per-day, skip in monthly
            allCheckIns.Count,
            newGoals, completedGoals,
            Math.Round(avgMood, 1),
            monthDiary.Count(d => d.Mood > 0),
            monthDiary.Count,
            income, expense,
            income - expense,
            tXp.Result,
            []);
    }
}
