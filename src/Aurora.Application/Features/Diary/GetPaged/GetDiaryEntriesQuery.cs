using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Diary.Common;
using MediatR;

namespace Aurora.Application.Features.Diary.GetPaged;

public record GetDiaryEntriesQuery(
    string UserId,
    string? Search,
    int? Mood,
    string? Tag,
    int Page,
    int PageSize) : IRequest<PagedResultDto<DiaryEntryDto>>;

public class GetDiaryEntriesHandler(IDiaryEntryRepository repo)
    : IRequestHandler<GetDiaryEntriesQuery, PagedResultDto<DiaryEntryDto>>
{
    public async Task<PagedResultDto<DiaryEntryDto>> Handle(GetDiaryEntriesQuery q, CancellationToken ct)
    {
        var (items, total) = await repo.GetPagedAsync(q.UserId, q.Search, q.Mood, q.Tag, q.Page, q.PageSize);
        var totalPages = q.PageSize > 0 ? (int)Math.Ceiling(total / (double)q.PageSize) : 1;
        return new PagedResultDto<DiaryEntryDto>(
            items.Select(e => e.ToDto()).ToList(), total, q.Page, q.PageSize, totalPages);
    }
}
