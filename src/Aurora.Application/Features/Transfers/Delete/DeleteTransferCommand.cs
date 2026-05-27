using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Transfers.Delete;

public record DeleteTransferCommand(string UserId, string Id) : IRequest;

public class DeleteTransferHandler(
    ITransferRepository transfers,
    IAccountRepository accounts) : IRequestHandler<DeleteTransferCommand>
{
    public async Task Handle(DeleteTransferCommand command, CancellationToken ct)
    {
        var transfer = await transfers.GetByIdAsync(command.Id, command.UserId)
            ?? throw new NotFoundException("Transferencia nao encontrada");

        var from = await accounts.GetByIdAsync(transfer.FromAccountId, command.UserId)
            ?? throw new ValidationException("Conta de origem invalida");
        var to = await accounts.GetByIdAsync(transfer.ToAccountId, command.UserId)
            ?? throw new ValidationException("Conta de destino invalida");

        var originReverted = false;
        var destinationReverted = false;
        try
        {
            from.CurrentBalance += transfer.Amount;
            await accounts.UpdateAsync(from);
            originReverted = true;

            to.CurrentBalance -= transfer.Amount;
            await accounts.UpdateAsync(to);
            destinationReverted = true;

            await transfers.DeleteAsync(transfer.Id, command.UserId);
        }
        catch
        {
            if (destinationReverted)
            {
                to.CurrentBalance += transfer.Amount;
                await accounts.UpdateAsync(to);
            }
            if (originReverted)
            {
                from.CurrentBalance -= transfer.Amount;
                await accounts.UpdateAsync(from);
            }
            throw;
        }
    }
}
