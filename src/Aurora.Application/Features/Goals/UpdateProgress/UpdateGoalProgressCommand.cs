using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Goals.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Goals.UpdateProgress;

public record UpdateGoalProgressCommand(string UserId, string Id, decimal CurrentValue) : IRequest<GoalDto>;

public class UpdateGoalProgressHandler(IGoalRepository repo, ITimelineEventRepository timelineRepo)
    : IRequestHandler<UpdateGoalProgressCommand, GoalDto>
{
    public async Task<GoalDto> Handle(UpdateGoalProgressCommand cmd, CancellationToken ct)
    {
        var goal = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Meta não encontrada.");

        var wasActive = goal.Status == GoalStatus.Active;
        goal.UpdateProgress(cmd.CurrentValue);
        await repo.UpdateAsync(goal, ct);

        var type = goal.Status == GoalStatus.Completed && wasActive
            ? TimelineEventType.GoalCompleted
            : TimelineEventType.GoalProgressed;

        await timelineRepo.AddFromModuleAsync(new Domain.Entities.TimelineEvent
        {
            UserId = cmd.UserId,
            Type = type,
            Area = goal.Area,
            Title = type == TimelineEventType.GoalCompleted
                ? $"Meta concluída: {goal.Title}"
                : $"Meta avançou: {goal.Title} — {goal.Progress}%",
            OccurredAt = DateTime.UtcNow,
            SourceModule = "Goals",
            SourceId = goal.Id,
            Visibility = Domain.Enums.TimelineVisibility.Private,
        });

        return goal.ToDto();
    }
}
