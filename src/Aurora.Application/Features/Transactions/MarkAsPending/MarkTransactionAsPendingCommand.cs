using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Transactions.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Transactions.MarkAsPending;

public record MarkTransactionAsPendingCommand(string UserId, string Id) : IRequest<TransactionDto>;

public class MarkTransactionAsPendingHandler(
    ITransactionRepository txRepo,
    IAccountRepository accRepo,
    ICacheService cache) : IRequestHandler<MarkTransactionAsPendingCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(MarkTransactionAsPendingCommand command, CancellationToken ct)
    {
        var tx = await txRepo.GetByIdAsync(command.Id, command.UserId)
            ?? throw new NotFoundException("Transacao nao encontrada");

        if (tx.Status == TransactionStatus.Paid)
        {
            var account = await accRepo.GetByIdAsync(tx.AccountId, command.UserId)
                ?? throw new ValidationException("Conta invalida");

            if (account.Type != AccountType.CreditCard)
            {
                account.ReverseTransaction(tx.Type, tx.Amount);
                await accRepo.UpdateAsync(account);
            }
        }

        tx.MarkAsPending(DateTime.UtcNow);
        await txRepo.UpdateAsync(tx);
        await cache.RemoveByPrefixAsync($"aurora:dashboard:{command.UserId}", ct);
        return tx.ToDto();
    }
}
