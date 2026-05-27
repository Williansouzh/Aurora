using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface ICreditCardInvoiceRepository
{
    Task<List<CreditCardInvoice>> GetByAccountAsync(string accountId, string userId);
    Task<CreditCardInvoice?> GetByPeriodAsync(string accountId, string userId, int month, int year);
    Task<CreditCardInvoice?> GetByIdAsync(string id, string userId);
    Task AddAsync(CreditCardInvoice invoice);
    Task UpdateAsync(CreditCardInvoice invoice);
    Task<decimal> SumOpenByAccountAsync(string accountId, string userId);
}
