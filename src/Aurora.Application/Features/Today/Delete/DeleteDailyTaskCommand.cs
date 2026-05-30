using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Today.Delete;

public record DeleteDailyTaskCommand(string UserId, string Id) : IRequest;

public class DeleteDailyTaskHandler(IDailyTaskRepository repo)
    : IRequestHandler<DeleteDailyTaskCommand>
{
    public async Task Handle(DeleteDailyTaskCommand cmd, CancellationToken ct)
    {
        var task = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Tarefa não encontrada.");

        await repo.DeleteAsync(task.Id, cmd.UserId, ct);
    }
}
