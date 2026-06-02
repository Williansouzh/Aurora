using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class StudyPracticeTaskRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<StudyPracticeTask>(context.StudyPracticeTasks, unitOfWork), IStudyPracticeTaskRepository
{
    public Task<List<StudyPracticeTask>> GetBySkillAsync(string userId, string skillId, StudyPracticeStatus? status, int limit, CancellationToken ct = default)
    {
        var filter = Builders<StudyPracticeTask>.Filter.Eq(x => x.UserId, userId) &
                     Builders<StudyPracticeTask>.Filter.Eq(x => x.SkillId, skillId);

        if (status is not null)
        {
            filter &= Builders<StudyPracticeTask>.Filter.Eq(x => x.Status, status.Value);
        }

        return Collection.Find(filter)
            .SortBy(x => x.DueDate)
            .ThenByDescending(x => x.CreatedAt)
            .Limit(Math.Clamp(limit, 1, 100))
            .ToListAsync(ct);
    }

    public Task<List<StudyPracticeTask>> GetRecentAsync(string userId, int limit, CancellationToken ct = default) =>
        Collection.Find(x => x.UserId == userId)
            .SortBy(x => x.Status)
            .ThenBy(x => x.DueDate)
            .ThenByDescending(x => x.CreatedAt)
            .Limit(Math.Clamp(limit, 1, 50))
            .ToListAsync(ct);

    public Task<int> CountOpenAsync(string userId, DateTime today, CancellationToken ct = default) =>
        Collection.CountDocumentsAsync(x =>
            x.UserId == userId &&
            x.Status != StudyPracticeStatus.Completed &&
            x.Status != StudyPracticeStatus.Cancelled &&
            x.DueDate <= today.AddDays(7), cancellationToken: ct)
            .ContinueWith(x => (int)x.Result, ct);

    public Task<int> CountCompletedThisWeekAsync(string userId, DateTime weekStart, DateTime weekEnd, CancellationToken ct = default) =>
        Collection.CountDocumentsAsync(x =>
            x.UserId == userId &&
            x.Status == StudyPracticeStatus.Completed &&
            x.CompletedAt >= weekStart &&
            x.CompletedAt < weekEnd, cancellationToken: ct)
            .ContinueWith(x => (int)x.Result, ct);

    public async Task<double> AverageDifficultyCompletedThisWeekAsync(string userId, DateTime weekStart, DateTime weekEnd, CancellationToken ct = default)
    {
        var result = await Collection.Aggregate()
            .Match(x => x.UserId == userId &&
                        x.Status == StudyPracticeStatus.Completed &&
                        x.CompletedAt >= weekStart &&
                        x.CompletedAt < weekEnd)
            .Group(x => x.UserId, g => new { Average = g.Average(x => x.Difficulty) })
            .FirstOrDefaultAsync(ct);

        return result?.Average ?? 0;
    }
}
