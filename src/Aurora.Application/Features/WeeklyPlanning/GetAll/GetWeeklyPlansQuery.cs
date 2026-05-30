using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.WeeklyPlanning.Common;
using MediatR;

namespace Aurora.Application.Features.WeeklyPlanning.GetAll;

public record GetWeeklyPlansQuery(string UserId, int Limit = 10) : IRequest<List<WeeklyPlanDto>>;

public class GetWeeklyPlansHandler(IWeeklyPlanRepository repo)
    : IRequestHandler<GetWeeklyPlansQuery, List<WeeklyPlanDto>>
{
    public async Task<List<WeeklyPlanDto>> Handle(GetWeeklyPlansQuery q, CancellationToken ct)
    {
        var plans = await repo.GetRecentAsync(q.UserId, q.Limit);
        return plans.Select(p => p.ToDto()).ToList();
    }
}
