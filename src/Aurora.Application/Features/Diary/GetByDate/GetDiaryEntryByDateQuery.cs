using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Diary.Common;
using MediatR;

namespace Aurora.Application.Features.Diary.GetByDate;

public record GetDiaryEntryByDateQuery(string UserId, DateTime Date) : IRequest<DiaryEntryDto?>;

public class GetDiaryEntryByDateHandler(IDiaryEntryRepository repo)
    : IRequestHandler<GetDiaryEntryByDateQuery, DiaryEntryDto?>
{
    public async Task<DiaryEntryDto?> Handle(GetDiaryEntryByDateQuery q, CancellationToken ct)
    {
        var entry = await repo.GetByDateAsync(q.UserId, q.Date.Date);
        return entry?.ToDto();
    }
}
