using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class StudyTopicRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<StudyTopic>(context.StudyTopics, unitOfWork), IStudyTopicRepository
{
    public Task<List<StudyTopic>> GetBySkillAsync(string userId, string skillId, CancellationToken ct = default) =>
        Collection.Find(x => x.UserId == userId && x.SkillId == skillId)
            .SortBy(x => x.Stage)
            .ThenByDescending(x => x.Importance)
            .ThenBy(x => x.Title)
            .ToListAsync(ct);
}

