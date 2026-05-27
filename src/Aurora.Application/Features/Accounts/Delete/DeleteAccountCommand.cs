using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Accounts.Delete;

public record DeleteAccountCommand(string UserId, string Id) : IRequest;

public class DeleteAccountHandler(
    IAccountRepository accounts,
    ITransactionRepository transactions) : IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand command, CancellationToken ct)
    {
        if (await transactions.ExistsByAccountIdAsync(command.Id, command.UserId))
        {
            throw new ConflictException("Conta possui transacoes vinculadas");
        }

        await accounts.DeleteAsync(command.Id, command.UserId);
    }
}
