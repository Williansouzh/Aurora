using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Studies.GetSessions;

public record GetStudySessionsQuery(string UserId, StudySessionStatus? Status, int Limit) : IRequest<List<StudySessionDto>>;

public class GetStudySessionsHandler(
    IStudySessionRepository sessions,
    IStudySkillRepository skills)
    : IRequestHandler<GetStudySessionsQuery, List<StudySessionDto>>
{
    public async Task<List<StudySessionDto>> Handle(GetStudySessionsQuery query, CancellationToken ct)
    {
        var items = await sessions.GetByStatusAsync(query.UserId, query.Status, Math.Clamp(query.Limit, 1, 50), ct);
        var skillIds = items.Select(x => x.SkillId).Distinct().ToHashSet();
        var skillMap = (await skills.GetByUserAsync(query.UserId, ct))
            .Where(x => skillIds.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x.Title);

        return items.Select(x => x.ToDto(skillMap.GetValueOrDefault(x.SkillId, "Estudo"))).ToList();
    }
}

