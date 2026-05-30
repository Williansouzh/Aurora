using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Today.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Today.Complete;

public record CompleteDailyTaskCommand(string UserId, string Id) : IRequest<DailyTaskDto>;

public class CompleteDailyTaskHandler(IDailyTaskRepository repo)
    : IRequestHandler<CompleteDailyTaskCommand, DailyTaskDto>
{
    public async Task<DailyTaskDto> Handle(CompleteDailyTaskCommand cmd, CancellationToken ct)
    {
        var task = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Tarefa não encontrada.");

        task.Complete();
        await repo.UpdateAsync(task, ct);
        return task.ToDto();
    }
}
