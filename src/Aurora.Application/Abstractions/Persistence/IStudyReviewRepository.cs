using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IStudyReviewRepository : IRepository<StudyReview>
{
    Task<List<StudyReview>> GetDueAsync(string userId, DateTime today, int limit, CancellationToken ct = default);
    Task<int> CountDueAsync(string userId, DateTime today, CancellationToken ct = default);
    Task<int> CountCompletedThisWeekAsync(string userId, DateTime weekStart, DateTime weekEnd, CancellationToken ct = default);
}

