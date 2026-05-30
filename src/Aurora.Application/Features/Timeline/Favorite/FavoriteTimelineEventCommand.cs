using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Timeline.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Timeline.Favorite;

public record FavoriteTimelineEventCommand(string UserId, string Id, bool Favorite) : IRequest<TimelineEventDto>;

public class FavoriteTimelineEventHandler(ITimelineEventRepository repo)
    : IRequestHandler<FavoriteTimelineEventCommand, TimelineEventDto>
{
    public async Task<TimelineEventDto> Handle(FavoriteTimelineEventCommand cmd, CancellationToken ct)
    {
        var evt = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Evento não encontrado.");

        evt.IsFavorite = cmd.Favorite;
        evt.UpdatedAt = DateTime.UtcNow;
        await repo.UpdateAsync(evt, ct);
        return evt.ToDto();
    }
}
