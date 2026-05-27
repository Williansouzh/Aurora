using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.CreditCardInvoices.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.CreditCardInvoices.GetByAccount;

public record GetCreditCardInvoicesQuery(string UserId, string AccountId) : IRequest<List<CreditCardInvoiceDto>>;

public class GetCreditCardInvoicesHandler(
    IAccountRepository accounts,
    ICreditCardInvoiceRepository invoices,
    ITransactionRepository transactions) : IRequestHandler<GetCreditCardInvoicesQuery, List<CreditCardInvoiceDto>>
{
    public async Task<List<CreditCardInvoiceDto>> Handle(GetCreditCardInvoicesQuery query, CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync(query.AccountId, query.UserId)
            ?? throw new NotFoundException("Conta nao encontrada");

        if (account.Type != AccountType.CreditCard)
        {
            throw new ValidationException("Conta nao e cartao de credito");
        }

        var list = await invoices.GetByAccountAsync(query.AccountId, query.UserId);

        var result = new List<CreditCardInvoiceDto>();
        foreach (var invoice in list)
        {
            result.Add(invoice.ToDto(await transactions.GetByInvoiceIdAsync(invoice.Id, query.UserId)));
        }
        return result;
    }
}
