using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Goals.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Goals.UpdateLinks;

public record UpdateGoalLinksCommand(
    string UserId,
    string Id,
    List<string> HabitIds,
    List<string> SkillIds) : IRequest<GoalDto>;

public class UpdateGoalLinksHandler(
    IGoalRepository repo,
    IHabitRepository habits,
    IStudySkillRepository skills) : IRequestHandler<UpdateGoalLinksCommand, GoalDto>
{
    public async Task<GoalDto> Handle(UpdateGoalLinksCommand cmd, CancellationToken ct)
    {
        var goal = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Meta não encontrada.");

        goal.SetLinks(cmd.HabitIds ?? [], cmd.SkillIds ?? []);
        await repo.UpdateAsync(goal, ct);

        var (habitNames, skillNames) = await GoalLinkResolver.LoadAsync(cmd.UserId, habits, skills, ct);
        return goal.ToDto(habitNames, skillNames);
    }
}
