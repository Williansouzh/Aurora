using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Goals.Delete;

public record DeleteGoalCommand(string UserId, string Id) : IRequest;

public class DeleteGoalHandler(IGoalRepository repo) : IRequestHandler<DeleteGoalCommand>
{
    public async Task Handle(DeleteGoalCommand cmd, CancellationToken ct)
    {
        var goal = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Meta não encontrada.");
        await repo.DeleteAsync(goal.Id, cmd.UserId, ct);
    }
}
