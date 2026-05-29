using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Transactions.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Transactions.MarkAsPaid;

public record MarkTransactionAsPaidCommand(string UserId, string Id) : IRequest<TransactionDto>;

public class MarkTransactionAsPaidHandler(
    ITransactionRepository txRepo,
    IAccountRepository accRepo,
    ICacheService cache) : IRequestHandler<MarkTransactionAsPaidCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(MarkTransactionAsPaidCommand command, CancellationToken ct)
    {
        var tx = await txRepo.GetByIdAsync(command.Id, command.UserId)
            ?? throw new NotFoundException("Transacao nao encontrada");

        if (tx.Status != TransactionStatus.Paid)
        {
            var account = await accRepo.GetByIdAsync(tx.AccountId, command.UserId)
                ?? throw new ValidationException("Conta invalida");

            if (account.Type != AccountType.CreditCard)
            {
                account.ApplyTransaction(tx.Type, tx.Amount);
                await accRepo.UpdateAsync(account);
            }

            tx.MarkAsPaid(DateTime.UtcNow);
            await txRepo.UpdateAsync(tx);
        }

        await cache.RemoveByPrefixAsync(CacheKeys.DashboardPrefix(command.UserId), ct);
        return tx.ToDto();
    }
}
