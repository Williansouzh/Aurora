using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Habits.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Habits.Pause;

public record PauseHabitCommand(string UserId, string Id, bool Pause) : IRequest<HabitDto>;

public class PauseHabitHandler(IHabitRepository repo)
    : IRequestHandler<PauseHabitCommand, HabitDto>
{
    public async Task<HabitDto> Handle(PauseHabitCommand cmd, CancellationToken ct)
    {
        var habit = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Hábito não encontrado.");

        if (cmd.Pause) habit.Pause();
        else habit.Resume();

        await repo.UpdateAsync(habit, ct);
        return habit.ToDto();
    }
}
