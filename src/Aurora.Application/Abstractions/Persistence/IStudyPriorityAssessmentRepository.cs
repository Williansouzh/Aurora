using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IStudyPriorityAssessmentRepository : IRepository<StudyPriorityAssessment>
{
    Task<StudyPriorityAssessment?> GetLatestForSkillAsync(string userId, string skillId, CancellationToken ct = default);
}

