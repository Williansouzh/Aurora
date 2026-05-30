using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Diary.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Diary.Update;

public record UpdateDiaryEntryCommand(
    string UserId,
    string Id,
    string Content,
    int Mood,
    List<string>? Tags) : IRequest<DiaryEntryDto>;

public class UpdateDiaryEntryHandler(IDiaryEntryRepository repo)
    : IRequestHandler<UpdateDiaryEntryCommand, DiaryEntryDto>
{
    public async Task<DiaryEntryDto> Handle(UpdateDiaryEntryCommand cmd, CancellationToken ct)
    {
        var entry = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Registro não encontrado.");

        entry.Update(cmd.Content, cmd.Mood, cmd.Tags ?? []);
        await repo.UpdateAsync(entry, ct);
        return entry.ToDto();
    }
}
