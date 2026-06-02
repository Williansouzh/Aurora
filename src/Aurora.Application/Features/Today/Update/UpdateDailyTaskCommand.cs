using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Today.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Today.Update;

public record UpdateDailyTaskCommand(
    string UserId,
    string Id,
    string Title,
    string? Notes,
    DailyTaskPriority Priority,
    DateTime? Date = null) : IRequest<DailyTaskDto>;

public class UpdateDailyTaskValidator : AbstractValidator<UpdateDailyTaskCommand>
{
    public UpdateDailyTaskValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => x.Notes is not null);
    }
}

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
        if (cmd.Date.HasValue)
        {
            task.Date = cmd.Date.Value.Date;
            task.IsBacklog = false;
        }
        task.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateAsync(task, ct);
        return task.ToDto();
    }
}
