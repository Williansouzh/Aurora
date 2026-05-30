using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Today.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Today.Reopen;

public record ReopenDailyTaskCommand(string UserId, string Id) : IRequest<DailyTaskDto>;

public class ReopenDailyTaskHandler(IDailyTaskRepository repo)
    : IRequestHandler<ReopenDailyTaskCommand, DailyTaskDto>
{
    public async Task<DailyTaskDto> Handle(ReopenDailyTaskCommand cmd, CancellationToken ct)
    {
        var task = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Tarefa não encontrada.");

        task.Reopen();
        await repo.UpdateAsync(task, ct);
        return task.ToDto();
    }
}
