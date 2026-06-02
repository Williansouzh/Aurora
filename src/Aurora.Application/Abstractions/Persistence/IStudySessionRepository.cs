using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Abstractions.Persistence;

public interface IStudySessionRepository : IRepository<StudySession>
{
    Task<List<StudySession>> GetRecentAsync(string userId, int limit, CancellationToken ct = default);
    Task<List<StudySession>> GetByStatusAsync(string userId, StudySessionStatus? status, int limit, CancellationToken ct = default);
    Task<int> SumCompletedMinutesThisWeekAsync(string userId, DateTime weekStart, DateTime weekEnd, CancellationToken ct = default);
    Task<int> CountCompletedAsync(string userId, DateTime from, DateTime to, CancellationToken ct = default);
}
