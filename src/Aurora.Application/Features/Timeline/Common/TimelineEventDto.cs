using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.Timeline.Common;

public record TimelineEventDto(
    string Id,
    TimelineEventType Type,
    LifeArea? Area,
    string Title,
    string? Description,
    DateTime OccurredAt,
    string? SourceModule,
    List<string> MediaUrls,
    bool IsHidden,
    bool IsFavorite,
    DateTime CreatedAt);

public static class TimelineMappingExtensions
{
    public static TimelineEventDto ToDto(this TimelineEvent e) => new(
        e.Id, e.Type, e.Area, e.Title, e.Description, e.OccurredAt,
        e.SourceModule, e.MediaUrls, e.IsHidden, e.IsFavorite, e.CreatedAt);
}
