using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Transactions.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Transactions.Delete;

public record DeleteTransactionCommand(string UserId, string Id) : IRequest;

public class DeleteTransactionHandler(
    ITransactionRepository txRepo,
    IAccountRepository accRepo,
    ICreditCardInvoiceRepository invoiceRepo,
    ICacheService cache) : IRequestHandler<DeleteTransactionCommand>
{
    public async Task Handle(DeleteTransactionCommand command, CancellationToken ct)
    {
        var tx = await txRepo.GetByIdAsync(command.Id, command.UserId)
            ?? throw new NotFoundException("Transacao nao encontrada");

        var account = await accRepo.GetByIdAsync(tx.AccountId, command.UserId)
            ?? throw new ValidationException("Conta invalida");

        if (account.Type == AccountType.CreditCard && !string.IsNullOrWhiteSpace(tx.CreditCardInvoiceId))
        {
            var invoice = await invoiceRepo.GetByIdAsync(tx.CreditCardInvoiceId, command.UserId);
            if (invoice is not null && invoice.Status != CreditCardInvoiceStatus.Paid)
            {
                invoice.TotalAmount = Math.Max(0, invoice.TotalAmount - tx.Amount);
                await invoiceRepo.UpdateAsync(invoice);
                account.CurrentBalance = await invoiceRepo.SumOpenByAccountAsync(account.Id, command.UserId);
                await accRepo.UpdateAsync(account);
            }
        }
        else if (tx.Status == TransactionStatus.Paid)
        {
            account.CurrentBalance -= TransactionMapper.Impact(tx.Type, tx.Amount);
            await accRepo.UpdateAsync(account);
        }

        await txRepo.DeleteAsync(tx.Id, command.UserId);
        await cache.RemoveByPrefixAsync($"aurora:dashboard:{command.UserId}", ct);
    }
}
