using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Studies.GetSkills;

public record GetStudySkillsQuery(string UserId, StudySkillStatus? Status) : IRequest<List<StudySkillDto>>;

public class GetStudySkillsHandler(IStudySkillRepository skills)
    : IRequestHandler<GetStudySkillsQuery, List<StudySkillDto>>
{
    public async Task<List<StudySkillDto>> Handle(GetStudySkillsQuery query, CancellationToken ct)
    {
        var items = await skills.GetByStatusAsync(query.UserId, query.Status, ct);
        return items.Select(x => x.ToDto()).ToList();
    }
}

