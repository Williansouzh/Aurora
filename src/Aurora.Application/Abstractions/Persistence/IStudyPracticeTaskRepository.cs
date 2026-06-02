using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Abstractions.Persistence;

public interface IStudyPracticeTaskRepository : IRepository<StudyPracticeTask>
{
    Task<List<StudyPracticeTask>> GetBySkillAsync(string userId, string skillId, StudyPracticeStatus? status, int limit, CancellationToken ct = default);
    Task<List<StudyPracticeTask>> GetRecentAsync(string userId, int limit, CancellationToken ct = default);
    Task<int> CountOpenAsync(string userId, DateTime today, CancellationToken ct = default);
    Task<int> CountCompletedThisWeekAsync(string userId, DateTime weekStart, DateTime weekEnd, CancellationToken ct = default);
    Task<double> AverageDifficultyCompletedThisWeekAsync(string userId, DateTime weekStart, DateTime weekEnd, CancellationToken ct = default);
}
