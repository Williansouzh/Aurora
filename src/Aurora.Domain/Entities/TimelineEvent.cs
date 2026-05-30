using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class TimelineEvent : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public TimelineEventType Type { get; set; }
    public LifeArea? Area { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? SourceModule { get; set; }
    public string? SourceId { get; set; }
    public TimelineVisibility Visibility { get; set; } = TimelineVisibility.Private;
    public List<string> MediaUrls { get; set; } = [];
    public bool IsHidden { get; set; }
    public bool IsFavorite { get; set; }
}
