using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Habits.Delete;

public record DeleteHabitCommand(string UserId, string Id) : IRequest;

public class DeleteHabitHandler(IHabitRepository repo)
    : IRequestHandler<DeleteHabitCommand>
{
    public async Task Handle(DeleteHabitCommand cmd, CancellationToken ct)
    {
        var habit = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Hábito não encontrado.");

        await repo.DeleteAsync(habit.Id, cmd.UserId, ct);
    }
}
