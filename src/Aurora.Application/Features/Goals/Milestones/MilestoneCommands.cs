using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Goals.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Goals.Milestones;

public record AddMilestoneCommand(string UserId, string GoalId, string Title, bool IsRequired) : IRequest<GoalDto>;
public record CompleteMilestoneCommand(string UserId, string GoalId, string MilestoneId) : IRequest<GoalDto>;
public record ReopenMilestoneCommand(string UserId, string GoalId, string MilestoneId) : IRequest<GoalDto>;
public record DeleteMilestoneCommand(string UserId, string GoalId, string MilestoneId) : IRequest<GoalDto>;

public class AddMilestoneHandler(IGoalRepository repo) : IRequestHandler<AddMilestoneCommand, GoalDto>
{
    public async Task<GoalDto> Handle(AddMilestoneCommand cmd, CancellationToken ct)
    {
        var goal = await repo.GetByIdAsync(cmd.GoalId, cmd.UserId, ct)
            ?? throw new NotFoundException("Meta não encontrada.");
        goal.AddMilestone(cmd.Title, cmd.IsRequired);
        await repo.UpdateAsync(goal, ct);
        return goal.ToDto();
    }
}

public class CompleteMilestoneHandler(IGoalRepository repo) : IRequestHandler<CompleteMilestoneCommand, GoalDto>
{
    public async Task<GoalDto> Handle(CompleteMilestoneCommand cmd, CancellationToken ct)
    {
        var goal = await repo.GetByIdAsync(cmd.GoalId, cmd.UserId, ct)
            ?? throw new NotFoundException("Meta não encontrada.");
        goal.CompleteMilestone(cmd.MilestoneId);
        await repo.UpdateAsync(goal, ct);
        return goal.ToDto();
    }
}

public class ReopenMilestoneHandler(IGoalRepository repo) : IRequestHandler<ReopenMilestoneCommand, GoalDto>
{
    public async Task<GoalDto> Handle(ReopenMilestoneCommand cmd, CancellationToken ct)
    {
        var goal = await repo.GetByIdAsync(cmd.GoalId, cmd.UserId, ct)
            ?? throw new NotFoundException("Meta não encontrada.");
        goal.ReopenMilestone(cmd.MilestoneId);
        await repo.UpdateAsync(goal, ct);
        return goal.ToDto();
    }
}

public class DeleteMilestoneHandler(IGoalRepository repo) : IRequestHandler<DeleteMilestoneCommand, GoalDto>
{
    public async Task<GoalDto> Handle(DeleteMilestoneCommand cmd, CancellationToken ct)
    {
        var goal = await repo.GetByIdAsync(cmd.GoalId, cmd.UserId, ct)
            ?? throw new NotFoundException("Meta não encontrada.");
        goal.Milestones.RemoveAll(m => m.Id == cmd.MilestoneId);
        goal.UpdatedAt = DateTime.UtcNow;
        await repo.UpdateAsync(goal, ct);
        return goal.ToDto();
    }
}
