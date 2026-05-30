using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Goals.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Goals.GetAll;

public record GetGoalsQuery(string UserId, GoalStatus? Status) : IRequest<List<GoalDto>>;

public class GetGoalsHandler(IGoalRepository repo) : IRequestHandler<GetGoalsQuery, List<GoalDto>>
{
    public async Task<List<GoalDto>> Handle(GetGoalsQuery q, CancellationToken ct)
    {
        var goals = await repo.GetByStatusAsync(q.UserId, q.Status);
        return goals.Select(g => g.ToDto()).ToList();
    }
}
