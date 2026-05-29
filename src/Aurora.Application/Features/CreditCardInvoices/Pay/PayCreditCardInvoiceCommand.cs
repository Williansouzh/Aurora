using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.CreditCardInvoices.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.CreditCardInvoices.Pay;

public record PayCreditCardInvoiceCommand(string UserId, string Id, string SourceAccountId, decimal Amount)
    : IRequest<CreditCardInvoiceDto>;

public class PayCreditCardInvoiceHandler(
    IAccountRepository accounts,
    ICreditCardInvoiceRepository invoices,
    ITransactionRepository transactions,
    ICacheService cache,
    IDateTimeProvider clock) : IRequestHandler<PayCreditCardInvoiceCommand, CreditCardInvoiceDto>
{
    public async Task<CreditCardInvoiceDto> Handle(PayCreditCardInvoiceCommand command, CancellationToken ct)
    {
        if (command.Amount <= 0)
        {
            throw new ValidationException("Valor de pagamento deve ser positivo");
        }

        var invoice = await invoices.GetByIdAsync(command.Id, command.UserId)
            ?? throw new NotFoundException("Fatura nao encontrada");

        if (invoice.Status == CreditCardInvoiceStatus.Paid)
        {
            throw new ValidationException("Fatura ja paga");
        }

        var card = await accounts.GetByIdAsync(invoice.AccountId, command.UserId)
            ?? throw new NotFoundException("Cartao nao encontrado");

        var source = await accounts.GetByIdAsync(command.SourceAccountId, command.UserId)
            ?? throw new ValidationException("Conta de debito invalida");

        if (source.Type == AccountType.CreditCard)
        {
            throw new ValidationException("Pagamento de fatura deve sair de uma conta de debito");
        }

        source.Debit(command.Amount);
        await accounts.UpdateAsync(source);

        invoice.MarkAsPaid(clock.UtcNow);
        await invoices.UpdateAsync(invoice);

        card.ReplaceCurrentBalance(await invoices.SumOpenByAccountAsync(card.Id, command.UserId));
        await accounts.UpdateAsync(card);

        var tx = new Transaction
        {
            UserId = command.UserId,
            AccountId = source.Id,
            CategoryId = string.Empty,
            Description = $"Pagamento fatura {card.Name} {invoice.Month:00}/{invoice.Year}",
            Amount = command.Amount,
            Type = TransactionType.Expense,
            Status = TransactionStatus.Paid,
            Date = clock.UtcNow,
            DueDate = invoice.DueDate,
            Notes = "Pagamento de fatura de cartao de credito",
            CreditCardInvoiceId = invoice.Id
        };

        await transactions.AddAsync(tx);
        await cache.RemoveByPrefixAsync(CacheKeys.DashboardPrefix(command.UserId), ct);

        return invoice.ToDto(await transactions.GetByInvoiceIdAsync(invoice.Id, command.UserId));
    }
}
