using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Accounts.Archive;

public record ArchiveAccountCommand(string UserId, string Id) : IRequest;

public class ArchiveAccountHandler(IAccountRepository accounts) : IRequestHandler<ArchiveAccountCommand>
{
    public async Task Handle(ArchiveAccountCommand command, CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync(command.Id, command.UserId)
            ?? throw new NotFoundException("Conta nao encontrada");

        account.IsArchived = true;
        await accounts.UpdateAsync(account);
    }
}
