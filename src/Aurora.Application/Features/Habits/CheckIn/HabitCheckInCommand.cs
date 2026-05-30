using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Habits.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Habits.CheckIn;

public record HabitCheckInCommand(
    string UserId,
    string HabitId,
    DateTime Date,
    HabitCheckInStatus Status,
    string? Note,
    string? PhotoUrl) : IRequest<HabitCheckInDto>;

public class HabitCheckInHandler(
    IHabitRepository habitRepo,
    IHabitCheckInRepository checkInRepo,
    ITimelineEventRepository timelineRepo,
    IXpService xpService)
    : IRequestHandler<HabitCheckInCommand, HabitCheckInDto>
{
    public async Task<HabitCheckInDto> Handle(HabitCheckInCommand cmd, CancellationToken ct)
    {
        var habit = await habitRepo.GetByIdAsync(cmd.HabitId, cmd.UserId, ct)
            ?? throw new NotFoundException("Hábito não encontrado.");

        var date = cmd.Date.Date;
        var existing = await checkInRepo.GetByHabitAndDateAsync(cmd.HabitId, cmd.UserId, date);
        if (existing is not null)
            throw new ConflictException("Já existe um check-in para este hábito nesta data.");

        var xpAmount = cmd.Status == HabitCheckInStatus.Done ? habit.XpReward : 0;

        var checkIn = new HabitCheckIn
        {
            UserId = cmd.UserId,
            HabitId = cmd.HabitId,
            Date = date,
            Status = cmd.Status,
            Note = cmd.Note,
            PhotoUrl = cmd.PhotoUrl,
            XpGenerated = xpAmount,
        };

        await checkInRepo.AddAsync(checkIn, ct);

        if (cmd.Status == HabitCheckInStatus.Done)
        {
            await UpdateStreakAsync(habit, date, ct);
            await xpService.AwardAsync(cmd.UserId, XpSource.HabitCheckIn, xpAmount,
                $"Check-in: {habit.Name}", ct);
        }

        await EmitTimelineEventAsync(habit, checkIn, ct);

        return checkIn.ToDto();
    }

    private async Task UpdateStreakAsync(Habit habit, DateTime date, CancellationToken ct)
    {
        var from = date.AddDays(-60);
        var checkIns = await checkInRepo.GetByHabitAsync(habit.Id, habit.UserId, from, date);

        var doneDates = checkIns
            .Where(c => c.Status == HabitCheckInStatus.Done)
            .Select(c => c.Date.Date)
            .ToHashSet();

        var streak = 0;
        var cursor = date.Date;
        while (doneDates.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        habit.UpdateStreak(streak);
        await habitRepo.UpdateAsync(habit, ct);
    }

    private async Task EmitTimelineEventAsync(Habit habit, HabitCheckIn checkIn, CancellationToken ct)
    {
        await timelineRepo.AddFromModuleAsync(new TimelineEvent
        {
            UserId = habit.UserId,
            Type = TimelineEventType.HabitCheckedIn,
            Area = habit.Area,
            Title = $"Check-in: {habit.Name}",
            Description = checkIn.Note,
            OccurredAt = checkIn.CreatedAt,
            SourceModule = "Habits",
            SourceId = checkIn.Id,
            Visibility = TimelineVisibility.Private,
        });
    }
}
