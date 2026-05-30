using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IWeeklyPlanRepository : IRepository<WeeklyPlan>
{
    Task<WeeklyPlan?> GetCurrentAsync(string userId);
    Task<List<WeeklyPlan>> GetRecentAsync(string userId, int limit = 10);
    Task<bool> ExistsForWeekAsync(string userId, DateTime weekStart);
}
