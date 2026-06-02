using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using MediatR;

namespace Aurora.Application.Features.Studies.GetTopics;

public record GetStudyTopicsQuery(string UserId, string SkillId) : IRequest<List<StudyTopicDto>>;

public class GetStudyTopicsHandler(IStudyTopicRepository topics, IStudySkillRepository skills)
    : IRequestHandler<GetStudyTopicsQuery, List<StudyTopicDto>>
{
    public async Task<List<StudyTopicDto>> Handle(GetStudyTopicsQuery query, CancellationToken ct)
    {
        var skill = await skills.GetByIdAsync(query.SkillId, query.UserId, ct)
            ?? throw new InvalidOperationException("Habilidade de estudo nao encontrada.");
        return (await topics.GetBySkillAsync(query.UserId, query.SkillId, ct))
            .Select(x => x.ToDto(skill.Title))
            .ToList();
    }
}

