using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Today.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Today.Update;

public record UpdateDailyTaskCommand(
    string UserId,
    string Id,
    string Title,
    string? Notes,
    DailyTaskPriority Priority) : IRequest<DailyTaskDto>;

public class UpdateDailyTaskHandler(IDailyTaskRepository repo)
    : IRequestHandler<UpdateDailyTaskCommand, DailyTaskDto>
{
    public async Task<DailyTaskDto> Handle(UpdateDailyTaskCommand cmd, CancellationToken ct)
    {
        var task = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Tarefa não encontrada.");

        task.Title = cmd.Title;
        task.Notes = cmd.Notes;
        task.Priority = cmd.Priority;
        task.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateAsync(task, ct);
        return task.ToDto();
    }
}
