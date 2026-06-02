using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Today.Common;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Today.Postpone;

public record PostponeOverdueTaskCommand(string UserId, string Id) : IRequest<DailyTaskDto>;

public class PostponeOverdueTaskValidator : AbstractValidator<PostponeOverdueTaskCommand>
{
    public PostponeOverdueTaskValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class PostponeOverdueTaskHandler(IDailyTaskRepository repo)
    : IRequestHandler<PostponeOverdueTaskCommand, DailyTaskDto>
{
    public async Task<DailyTaskDto> Handle(PostponeOverdueTaskCommand cmd, CancellationToken ct)
    {
        var task = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Tarefa não encontrada.");

        if (task.Date.Date >= DateTime.UtcNow.Date)
            throw new DomainException("Tarefa não está vencida.");

        task.Date = DateTime.UtcNow.Date;
        task.IsBacklog = false;
        task.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateAsync(task, ct);
        return task.ToDto();
    }
}
