using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Studies.GetPracticeTasks;

public record GetStudyPracticeTasksQuery(string UserId, string SkillId, StudyPracticeStatus? Status, int Limit)
    : IRequest<List<StudyPracticeTaskDto>>;

public class GetStudyPracticeTasksHandler(
    IStudyPracticeTaskRepository practices,
    IStudySkillRepository skills)
    : IRequestHandler<GetStudyPracticeTasksQuery, List<StudyPracticeTaskDto>>
{
    public async Task<List<StudyPracticeTaskDto>> Handle(GetStudyPracticeTasksQuery query, CancellationToken ct)
    {
        var skill = await skills.GetByIdAsync(query.SkillId, query.UserId, ct)
            ?? throw new InvalidOperationException("Habilidade de estudo nao encontrada.");

        return (await practices.GetBySkillAsync(query.UserId, query.SkillId, query.Status, query.Limit, ct))
            .Select(x => x.ToDto(skill.Title))
            .ToList();
    }
}
