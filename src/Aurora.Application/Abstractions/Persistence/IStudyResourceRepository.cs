using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IStudyResourceRepository : IRepository<StudyResource>
{
    Task<List<StudyResource>> GetBySkillAsync(string userId, string skillId, CancellationToken ct = default);
}

