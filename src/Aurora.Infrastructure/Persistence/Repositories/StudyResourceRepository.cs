using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class StudyResourceRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<StudyResource>(context.StudyResources, unitOfWork), IStudyResourceRepository
{
    public Task<List<StudyResource>> GetBySkillAsync(string userId, string skillId, CancellationToken ct = default) =>
        Collection.Find(x => x.UserId == userId && x.SkillId == skillId)
            .SortBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(ct);
}

