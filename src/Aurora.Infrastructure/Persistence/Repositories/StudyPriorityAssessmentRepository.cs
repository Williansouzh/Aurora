using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class StudyPriorityAssessmentRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<StudyPriorityAssessment>(context.StudyPriorityAssessments, unitOfWork), IStudyPriorityAssessmentRepository
{
    public async Task<StudyPriorityAssessment?> GetLatestForSkillAsync(string userId, string skillId, CancellationToken ct = default) =>
        await Collection.Find(x => x.UserId == userId && x.SkillId == skillId)
            .SortByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
}
