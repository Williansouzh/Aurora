using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Abstractions.Persistence;

public interface IDailyTaskRepository : IRepository<DailyTask>
{
    Task<List<DailyTask>> GetByDateAsync(string userId, DateTime date);
    Task<List<DailyTask>> GetOverdueAsync(string userId, DateTime before);
    Task<(List<DailyTask> Items, int TotalCount)> GetBacklogPagedAsync(string userId, int page, int pageSize);
    Task<DailyTask?> GetBySourceAsync(string userId, string sourceModule, string sourceId, CancellationToken ct = default);
}
