using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Diary.Delete;

public record DeleteDiaryEntryCommand(string UserId, string Id) : IRequest;

public class DeleteDiaryEntryHandler(IDiaryEntryRepository repo)
    : IRequestHandler<DeleteDiaryEntryCommand>
{
    public async Task Handle(DeleteDiaryEntryCommand cmd, CancellationToken ct)
    {
        var entry = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Registro não encontrado.");
        await repo.DeleteAsync(entry.Id, cmd.UserId, ct);
    }
}
