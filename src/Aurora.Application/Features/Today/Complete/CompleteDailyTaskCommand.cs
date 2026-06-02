using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Today.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Today.Complete;

public record CompleteDailyTaskCommand(string UserId, string Id) : IRequest<DailyTaskDto>;

public class CompleteDailyTaskValidator : AbstractValidator<CompleteDailyTaskCommand>
{
    public CompleteDailyTaskValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class CompleteDailyTaskHandler(
    IDailyTaskRepository repo,
    ITimelineEventRepository timelineRepo,
    IXpService xpService)
    : IRequestHandler<CompleteDailyTaskCommand, DailyTaskDto>
{
    private const int TaskXp = 5;

    public async Task<DailyTaskDto> Handle(CompleteDailyTaskCommand cmd, CancellationToken ct)
    {
        var task = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Tarefa não encontrada.");

        task.Complete();
        await repo.UpdateAsync(task, ct);

        await xpService.AwardAsync(cmd.UserId, XpSource.TaskCompleted, TaskXp,
            $"Tarefa concluída: {task.Title}", ct);

        await timelineRepo.AddFromModuleAsync(new TimelineEvent
        {
            UserId = cmd.UserId,
            Type = TimelineEventType.TaskCompleted,
            Area = LifeArea.Projects,
            Title = $"Tarefa concluída: {task.Title}",
            OccurredAt = task.CompletedAt ?? DateTime.UtcNow,
            SourceModule = "Today",
            SourceId = task.Id,
            Visibility = TimelineVisibility.Private,
        });

        return task.ToDto();
    }
}
