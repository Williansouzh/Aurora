using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Timeline.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Timeline.Hide;

public record HideTimelineEventCommand(string UserId, string Id, bool Hide) : IRequest<TimelineEventDto>;

public class HideTimelineEventHandler(ITimelineEventRepository repo)
    : IRequestHandler<HideTimelineEventCommand, TimelineEventDto>
{
    public async Task<TimelineEventDto> Handle(HideTimelineEventCommand cmd, CancellationToken ct)
    {
        var evt = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Evento não encontrado.");

        evt.IsHidden = cmd.Hide;
        evt.UpdatedAt = DateTime.UtcNow;
        await repo.UpdateAsync(evt, ct);
        return evt.ToDto();
    }
}
