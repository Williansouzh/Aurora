using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.WeeklyPlanning.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.WeeklyPlanning.Close;

public record CloseWeeklyPlanCommand(string UserId, string Id, string? Review) : IRequest<WeeklyPlanDto>;

public class CloseWeeklyPlanHandler(IWeeklyPlanRepository repo, ITimelineEventRepository timelineRepo, IXpService xp)
    : IRequestHandler<CloseWeeklyPlanCommand, WeeklyPlanDto>
{
    public async Task<WeeklyPlanDto> Handle(CloseWeeklyPlanCommand cmd, CancellationToken ct)
    {
        var plan = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Plano não encontrado.");

        plan.Close(cmd.Review);
        await repo.UpdateAsync(plan, ct);

        await timelineRepo.AddFromModuleAsync(new Domain.Entities.TimelineEvent
        {
            UserId = cmd.UserId,
            Type = TimelineEventType.WeeklyReviewClosed,
            Title = $"Semana encerrada: {plan.WeekStart:dd/MM} – {plan.WeekEnd:dd/MM}",
            Description = cmd.Review,
            OccurredAt = DateTime.UtcNow,
            SourceModule = "WeeklyPlanning",
            SourceId = plan.Id,
            Visibility = TimelineVisibility.Private,
        });

        await xp.AwardAsync(cmd.UserId, XpSource.WeeklyClose, plan.XpGenerated, "Semana encerrada", ct);
        return plan.ToDto();
    }
}
