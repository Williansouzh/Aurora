using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.WeeklyPlanning.Common;
using MediatR;

namespace Aurora.Application.Features.WeeklyPlanning.GetCurrent;

public record GetCurrentWeeklyPlanQuery(string UserId) : IRequest<WeeklyPlanDto?>;

public class GetCurrentWeeklyPlanHandler(IWeeklyPlanRepository repo)
    : IRequestHandler<GetCurrentWeeklyPlanQuery, WeeklyPlanDto?>
{
    public async Task<WeeklyPlanDto?> Handle(GetCurrentWeeklyPlanQuery q, CancellationToken ct)
    {
        var plan = await repo.GetCurrentAsync(q.UserId);
        return plan?.ToDto();
    }
}
