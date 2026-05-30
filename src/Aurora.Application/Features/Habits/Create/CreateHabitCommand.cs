using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Habits.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Habits.Create;

public record CreateHabitCommand(
    string UserId,
    string Name,
    string? Description,
    LifeArea Area,
    HabitFrequencyType FrequencyType,
    List<int>? DaysOfWeek,
    int TimesPerWeek,
    HabitDifficulty Difficulty) : IRequest<HabitDto>;

public class CreateHabitValidator : AbstractValidator<CreateHabitCommand>
{
    public CreateHabitValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TimesPerWeek).InclusiveBetween(1, 7);
    }
}

public class CreateHabitHandler(IHabitRepository repo)
    : IRequestHandler<CreateHabitCommand, HabitDto>
{
    public async Task<HabitDto> Handle(CreateHabitCommand cmd, CancellationToken ct)
    {
        var habit = new Habit
        {
            UserId = cmd.UserId,
            Name = cmd.Name,
            Description = cmd.Description,
            Area = cmd.Area,
            FrequencyType = cmd.FrequencyType,
            DaysOfWeek = cmd.DaysOfWeek ?? [],
            TimesPerWeek = cmd.TimesPerWeek,
            Difficulty = cmd.Difficulty,
            XpReward = Habit.XpForDifficulty(cmd.Difficulty),
        };

        await repo.AddAsync(habit, ct);
        return habit.ToDto();
    }
}
