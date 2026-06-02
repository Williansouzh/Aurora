using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Abstractions.Persistence;

public interface IStudySkillRepository : IRepository<StudySkill>
{
    Task<List<StudySkill>> GetByStatusAsync(string userId, StudySkillStatus? status, CancellationToken ct = default);
    Task<List<StudySkill>> GetActiveAsync(string userId, CancellationToken ct = default);
    Task<int> CountActiveAsync(string userId, CancellationToken ct = default);
}

