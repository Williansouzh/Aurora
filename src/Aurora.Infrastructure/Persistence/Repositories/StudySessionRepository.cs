using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class StudySessionRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<StudySession>(context.StudySessions, unitOfWork), IStudySessionRepository
{
    public Task<List<StudySession>> GetRecentAsync(string userId, int limit, CancellationToken ct = default) =>
        Collection.Find(x => x.UserId == userId)
            .SortByDescending(x => x.Date)
            .ThenByDescending(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync(ct);

    public Task<List<StudySession>> GetByStatusAsync(string userId, StudySessionStatus? status, int limit, CancellationToken ct = default)
    {
        var filter = status is null
            ? Builders<StudySession>.Filter.Eq(x => x.UserId, userId)
            : Builders<StudySession>.Filter.Eq(x => x.UserId, userId) &
              Builders<StudySession>.Filter.Eq(x => x.Status, status.Value);

        return Collection.Find(filter)
            .SortByDescending(x => x.Date)
            .ThenByDescending(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync(ct);
    }

    public async Task<int> SumCompletedMinutesThisWeekAsync(string userId, DateTime weekStart, DateTime weekEnd, CancellationToken ct = default)
    {
        var result = await Collection.Aggregate()
            .Match(x => x.UserId == userId &&
                        x.Status == StudySessionStatus.Completed &&
                        x.Date >= weekStart &&
                        x.Date < weekEnd)
            .Group(x => x.UserId, g => new { Minutes = g.Sum(x => x.ActualMinutes ?? 0) })
            .FirstOrDefaultAsync(ct);

        return result?.Minutes ?? 0;
    }

    public async Task<int> CountCompletedAsync(string userId, DateTime from, DateTime to, CancellationToken ct = default) =>
        (int)await Collection.CountDocumentsAsync(
            x => x.UserId == userId &&
                 x.Status == StudySessionStatus.Completed &&
                 x.Date >= from &&
                 x.Date < to,
            cancellationToken: ct);
}
