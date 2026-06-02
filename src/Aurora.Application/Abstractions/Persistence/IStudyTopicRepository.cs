using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IStudyTopicRepository : IRepository<StudyTopic>
{
    Task<List<StudyTopic>> GetBySkillAsync(string userId, string skillId, CancellationToken ct = default);
}

