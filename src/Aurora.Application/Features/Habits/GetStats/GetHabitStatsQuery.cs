using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Habits.GetStats;

public record GetHabitStatsQuery(string UserId, string HabitId, int Year, int Month) : IRequest<HabitStatsDto>;

public record HabitStatsDto(
    int CurrentStreak,
    int BestStreak,
    int DoneThisMonth,
    int TotalThisMonth,
    List<CheckInDayDto> Calendar);

public record CheckInDayDto(DateTime Date, HabitCheckInStatus? Status);

public class GetHabitStatsHandler(
    IHabitRepository habitRepo,
    IHabitCheckInRepository checkInRepo)
    : IRequestHandler<GetHabitStatsQuery, HabitStatsDto>
{
    public async Task<HabitStatsDto> Handle(GetHabitStatsQuery query, CancellationToken ct)
    {
        var habit = await habitRepo.GetByIdAsync(query.HabitId, query.UserId, ct)
            ?? throw new Aurora.Domain.Exceptions.NotFoundException("Hábito não encontrado.");

        var from = new DateTime(query.Year, query.Month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        var checkIns = await checkInRepo.GetByHabitAsync(query.HabitId, query.UserId, from, to);
        var byDate = checkIns.ToDictionary(c => c.Date.Date, c => c.Status);

        var calendar = Enumerable.Range(0, (to - from).Days + 1)
            .Select(i => from.AddDays(i).Date)
            .Select(d => new CheckInDayDto(d, byDate.TryGetValue(d, out var s) ? s : null))
            .ToList();

        var done = checkIns.Count(c => c.Status == HabitCheckInStatus.Done);
        var total = (to - from).Days + 1;

        return new HabitStatsDto(habit.CurrentStreak, habit.BestStreak, done, total, calendar);
    }
}
