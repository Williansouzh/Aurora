using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.CreditCardInvoices.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.CreditCardInvoices.GetByPeriod;

public record GetCreditCardInvoiceQuery(string UserId, string AccountId, int Month, int Year)
    : IRequest<CreditCardInvoiceDto>;

public class GetCreditCardInvoiceHandler(
    IAccountRepository accounts,
    ICreditCardInvoiceRepository invoices,
    ITransactionRepository transactions) : IRequestHandler<GetCreditCardInvoiceQuery, CreditCardInvoiceDto>
{
    public async Task<CreditCardInvoiceDto> Handle(GetCreditCardInvoiceQuery query, CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync(query.AccountId, query.UserId)
            ?? throw new NotFoundException("Conta nao encontrada");

        if (account.Type != AccountType.CreditCard)
        {
            throw new ValidationException("Conta nao e cartao de credito");
        }

        var invoice = await invoices.GetByPeriodAsync(query.AccountId, query.UserId, query.Month, query.Year);
        if (invoice is null)
        {
            invoice = new CreditCardInvoice
            {
                UserId = query.UserId,
                AccountId = query.AccountId,
                Month = query.Month,
                Year = query.Year,
                DueDate = CreditCardInvoiceCalculator.DueDate(query.Month, query.Year, account.DueDay)
            };
            await invoices.AddAsync(invoice);
        }

        return invoice.ToDto(await transactions.GetByInvoiceIdAsync(invoice.Id, query.UserId));
    }
}
