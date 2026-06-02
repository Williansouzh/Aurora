using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class StudyReviewRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<StudyReview>(context.StudyReviews, unitOfWork), IStudyReviewRepository
{
    public Task<List<StudyReview>> GetDueAsync(string userId, DateTime today, int limit, CancellationToken ct = default) =>
        Collection.Find(x => x.UserId == userId &&
                             x.Status == StudyReviewStatus.Pending &&
                             x.DueDate <= today)
            .SortBy(x => x.DueDate)
            .ThenBy(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync(ct);

    public async Task<int> CountDueAsync(string userId, DateTime today, CancellationToken ct = default) =>
        (int)await Collection.CountDocumentsAsync(
            x => x.UserId == userId &&
                 x.Status == StudyReviewStatus.Pending &&
                 x.DueDate <= today,
            cancellationToken: ct);

    public async Task<int> CountCompletedThisWeekAsync(string userId, DateTime weekStart, DateTime weekEnd, CancellationToken ct = default) =>
        (int)await Collection.CountDocumentsAsync(
            x => x.UserId == userId &&
                 x.Status == StudyReviewStatus.Completed &&
                 x.CompletedAt >= weekStart &&
                 x.CompletedAt < weekEnd,
            cancellationToken: ct);
}

