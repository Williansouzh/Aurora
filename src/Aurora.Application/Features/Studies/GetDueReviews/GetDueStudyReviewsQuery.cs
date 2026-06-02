using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using MediatR;

namespace Aurora.Application.Features.Studies.GetDueReviews;

public record GetDueStudyReviewsQuery(string UserId, int Limit) : IRequest<List<StudyReviewDto>>;

public class GetDueStudyReviewsHandler(
    IStudyReviewRepository reviews,
    IStudySkillRepository skills)
    : IRequestHandler<GetDueStudyReviewsQuery, List<StudyReviewDto>>
{
    public async Task<List<StudyReviewDto>> Handle(GetDueStudyReviewsQuery query, CancellationToken ct)
    {
        var due = await reviews.GetDueAsync(query.UserId, DateTime.UtcNow.Date, Math.Clamp(query.Limit, 1, 50), ct);
        var skillIds = due.Select(x => x.SkillId).Distinct().ToHashSet();
        var skillMap = (await skills.GetByUserAsync(query.UserId, ct))
            .Where(x => skillIds.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x.Title);

        return due.Select(x => x.ToDto(skillMap.GetValueOrDefault(x.SkillId, "Estudo"))).ToList();
    }
}

