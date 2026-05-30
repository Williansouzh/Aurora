using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.WeeklyPlanning.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.WeeklyPlanning.Update;

public record UpdateWeeklyPlanCommand(
    string UserId,
    string Id,
    string? MainFocus,
    List<string>? LinkedGoalIds,
    List<string>? Priorities,
    string? Notes) : IRequest<WeeklyPlanDto>;

public class UpdateWeeklyPlanHandler(IWeeklyPlanRepository repo)
    : IRequestHandler<UpdateWeeklyPlanCommand, WeeklyPlanDto>
{
    public async Task<WeeklyPlanDto> Handle(UpdateWeeklyPlanCommand cmd, CancellationToken ct)
    {
        var plan = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Plano não encontrado.");

        plan.MainFocus = cmd.MainFocus;
        plan.LinkedGoalIds = cmd.LinkedGoalIds ?? [];
        plan.Priorities = cmd.Priorities ?? [];
        plan.Notes = cmd.Notes;
        plan.UpdatedAt = DateTime.UtcNow;

        if (plan.Status == Domain.Enums.WeeklyPlanStatus.NotStarted)
            plan.Start();

        await repo.UpdateAsync(plan, ct);
        return plan.ToDto();
    }
}
