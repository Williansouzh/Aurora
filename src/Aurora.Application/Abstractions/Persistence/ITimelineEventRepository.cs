using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Abstractions.Persistence;

public interface ITimelineEventRepository : IRepository<TimelineEvent>
{
    Task<(List<TimelineEvent> Items, int TotalCount)> GetPagedAsync(
        string userId,
        TimelineEventType? type,
        LifeArea? area,
        DateTime? from,
        DateTime? to,
        bool favoritesOnly,
        int page,
        int pageSize);

    Task<List<TimelineEvent>> GetRecentAsync(string userId, int limit = 5);
    Task AddFromModuleAsync(TimelineEvent evt);
}
