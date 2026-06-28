using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Goals.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Goals.GetById;

public record GetGoalByIdQuery(string UserId, string Id) : IRequest<GoalDto>;

public class GetGoalByIdHandler(
    IGoalRepository repo,
    IHabitRepository habits,
    IStudySkillRepository skills) : IRequestHandler<GetGoalByIdQuery, GoalDto>
{
    public async Task<GoalDto> Handle(GetGoalByIdQuery q, CancellationToken ct)
    {
        var goal = await repo.GetByIdAsync(q.Id, q.UserId, ct)
            ?? throw new NotFoundException("Meta não encontrada.");
        var (habitNames, skillNames) = await GoalLinkResolver.LoadAsync(q.UserId, habits, skills, ct);
        return goal.ToDto(habitNames, skillNames);
    }
}
