using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Habits.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Habits.Update;

public record UpdateHabitCommand(
    string UserId,
    string Id,
    string Name,
    string? Description,
    LifeArea Area,
    HabitFrequencyType FrequencyType,
    List<int>? DaysOfWeek,
    int TimesPerWeek,
    HabitDifficulty Difficulty) : IRequest<HabitDto>;

public class UpdateHabitHandler(IHabitRepository repo)
    : IRequestHandler<UpdateHabitCommand, HabitDto>
{
    public async Task<HabitDto> Handle(UpdateHabitCommand cmd, CancellationToken ct)
    {
        var habit = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Hábito não encontrado.");

        habit.Name = cmd.Name;
        habit.Description = cmd.Description;
        habit.Area = cmd.Area;
        habit.FrequencyType = cmd.FrequencyType;
        habit.DaysOfWeek = cmd.DaysOfWeek ?? [];
        habit.TimesPerWeek = cmd.TimesPerWeek;
        habit.Difficulty = cmd.Difficulty;
        habit.XpReward = Habit.XpForDifficulty(cmd.Difficulty);
        habit.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateAsync(habit, ct);
        return habit.ToDto();
    }
}
