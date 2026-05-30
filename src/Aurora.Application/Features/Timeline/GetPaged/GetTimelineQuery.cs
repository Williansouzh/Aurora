using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Timeline.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Timeline.GetPaged;

public record GetTimelineQuery(
    string UserId,
    TimelineEventType? Type,
    LifeArea? Area,
    DateTime? From,
    DateTime? To,
    bool FavoritesOnly,
    int Page,
    int PageSize) : IRequest<PagedResultDto<TimelineEventDto>>;

public class GetTimelineHandler(ITimelineEventRepository repo)
    : IRequestHandler<GetTimelineQuery, PagedResultDto<TimelineEventDto>>
{
    public async Task<PagedResultDto<TimelineEventDto>> Handle(GetTimelineQuery query, CancellationToken ct)
    {
        var (items, total) = await repo.GetPagedAsync(
            query.UserId, query.Type, query.Area,
            query.From, query.To, query.FavoritesOnly,
            query.Page, query.PageSize);

        var dtos = items.Select(e => e.ToDto()).ToList();
        var totalPages = query.PageSize > 0 ? (int)Math.Ceiling(total / (double)query.PageSize) : 1;
        return new PagedResultDto<TimelineEventDto>(dtos, total, query.Page, query.PageSize, totalPages);
    }
}
