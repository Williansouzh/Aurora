using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Goals.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Goals.ChangeStatus;

public record ChangeGoalStatusCommand(
    string UserId,
    string Id,
    string Action,
    string? Reason) : IRequest<GoalDto>;

public class ChangeGoalStatusHandler(IGoalRepository repo) : IRequestHandler<ChangeGoalStatusCommand, GoalDto>
{
    public async Task<GoalDto> Handle(ChangeGoalStatusCommand cmd, CancellationToken ct)
    {
        var goal = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Meta não encontrada.");

        switch (cmd.Action.ToLower())
        {
            case "complete": goal.ForceComplete(cmd.Reason ?? ""); break;
            case "pause":    goal.Pause();   break;
            case "resume":   goal.Resume();  break;
            case "cancel":   goal.Cancel();  break;
            default: throw new ValidationException($"Ação inválida: {cmd.Action}");
        }

        await repo.UpdateAsync(goal, ct);
        return goal.ToDto();
    }
}
