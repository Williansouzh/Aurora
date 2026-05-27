using Aurora.Application.Features.Transactions.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.CreditCardInvoices.Common;

public record CreditCardInvoiceDto(
    string Id,
    string AccountId,
    int Month,
    int Year,
    decimal TotalAmount,
    CreditCardInvoiceStatus Status,
    DateTime DueDate,
    DateTime? PaidAt,
    List<TransactionDto> Transactions);

public static class CreditCardInvoiceMapper
{
    public static CreditCardInvoiceDto ToDto(this CreditCardInvoice invoice, List<Transaction> transactions) =>
        new(
            invoice.Id,
            invoice.AccountId,
            invoice.Month,
            invoice.Year,
            invoice.TotalAmount,
            invoice.Status,
            invoice.DueDate,
            invoice.PaidAt,
            transactions.Select(t => t.ToDto()).ToList());
}
