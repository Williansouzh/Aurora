using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using MediatR;

namespace Aurora.Application.Features.Studies.GetResources;

public record GetStudyResourcesQuery(string UserId, string SkillId) : IRequest<List<StudyResourceDto>>;

public class GetStudyResourcesHandler(IStudyResourceRepository resources, IStudySkillRepository skills)
    : IRequestHandler<GetStudyResourcesQuery, List<StudyResourceDto>>
{
    public async Task<List<StudyResourceDto>> Handle(GetStudyResourcesQuery query, CancellationToken ct)
    {
        var skill = await skills.GetByIdAsync(query.SkillId, query.UserId, ct)
            ?? throw new InvalidOperationException("Habilidade de estudo nao encontrada.");
        return (await resources.GetBySkillAsync(query.UserId, query.SkillId, ct))
            .Select(x => x.ToDto(skill.Title))
            .ToList();
    }
}

