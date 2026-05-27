using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Transfers.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Transfers.Create;

public record CreateTransferCommand(
    string UserId,
    string FromAccountId,
    string ToAccountId,
    decimal Amount,
    DateTime Date,
    string Description) : IRequest<TransferDto>;

public class CreateTransferHandler(
    ITransferRepository transfers,
    IAccountRepository accounts) : IRequestHandler<CreateTransferCommand, TransferDto>
{
    public async Task<TransferDto> Handle(CreateTransferCommand command, CancellationToken ct)
    {
        if (command.Amount <= 0)
        {
            throw new ValidationException("Valor da transferencia deve ser positivo");
        }

        if (command.FromAccountId == command.ToAccountId)
        {
            throw new ValidationException("Conta de origem e destino devem ser diferentes");
        }

        var from = await accounts.GetByIdAsync(command.FromAccountId, command.UserId)
            ?? throw new ValidationException("Conta de origem invalida");
        var to = await accounts.GetByIdAsync(command.ToAccountId, command.UserId)
            ?? throw new ValidationException("Conta de destino invalida");

        if (from.Type == AccountType.CreditCard || to.Type == AccountType.CreditCard)
        {
            throw new ValidationException("Transferencia nao usa cartao de credito");
        }

        var originUpdated = false;
        var destinationUpdated = false;
        try
        {
            from.CurrentBalance -= command.Amount;
            await accounts.UpdateAsync(from);
            originUpdated = true;

            to.CurrentBalance += command.Amount;
            await accounts.UpdateAsync(to);
            destinationUpdated = true;

            var transfer = new Transfer
            {
                UserId = command.UserId,
                FromAccountId = from.Id,
                ToAccountId = to.Id,
                Amount = command.Amount,
                Date = command.Date,
                Description = command.Description,
                Status = TransferStatus.Completed
            };

            await transfers.AddAsync(transfer);
            return transfer.ToDto();
        }
        catch
        {
            if (destinationUpdated)
            {
                to.CurrentBalance -= command.Amount;
                await accounts.UpdateAsync(to);
            }
            if (originUpdated)
            {
                from.CurrentBalance += command.Amount;
                await accounts.UpdateAsync(from);
            }
            throw;
        }
    }
}
