using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class StudySkillRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<StudySkill>(context.StudySkills, unitOfWork), IStudySkillRepository
{
    public Task<List<StudySkill>> GetByStatusAsync(string userId, StudySkillStatus? status, CancellationToken ct = default)
    {
        var filter = status is null
            ? Builders<StudySkill>.Filter.Eq(x => x.UserId, userId)
            : Builders<StudySkill>.Filter.Eq(x => x.UserId, userId) &
              Builders<StudySkill>.Filter.Eq(x => x.Status, status.Value);

        return Collection.Find(filter)
            .SortBy(x => x.Status)
            .ThenBy(x => x.PriorityRank)
            .ThenByDescending(x => x.PriorityScore)
            .ThenBy(x => x.Title)
            .ToListAsync(ct);
    }

    public Task<List<StudySkill>> GetActiveAsync(string userId, CancellationToken ct = default) =>
        Collection.Find(x => x.UserId == userId && x.Status == StudySkillStatus.Active)
            .SortBy(x => x.PriorityRank)
            .ThenByDescending(x => x.PriorityScore)
            .ToListAsync(ct);

    public async Task<int> CountActiveAsync(string userId, CancellationToken ct = default) =>
        (int)await Collection.CountDocumentsAsync(
            x => x.UserId == userId && x.Status == StudySkillStatus.Active,
            cancellationToken: ct);
}
