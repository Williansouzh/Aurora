using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class CreditCardInvoiceRepository(MongoContext context)
    : MongoRepositoryBase<CreditCardInvoice>(context.CreditCardInvoices), ICreditCardInvoiceRepository
{
    public Task<List<CreditCardInvoice>> GetByAccountAsync(string accountId, string userId) =>
        Collection.Find(x => x.AccountId == accountId && x.UserId == userId)
            .SortByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToListAsync();

    public Task<CreditCardInvoice?> GetByPeriodAsync(string accountId, string userId, int month, int year) =>
        Collection.Find(x => x.AccountId == accountId && x.UserId == userId && x.Month == month && x.Year == year)
            .FirstOrDefaultAsync()!;

    public Task<CreditCardInvoice?> GetByIdAsync(string id, string userId) => base.GetByIdAsync(id, userId);

    public Task AddAsync(CreditCardInvoice invoice) => base.AddAsync(invoice);

    public Task UpdateAsync(CreditCardInvoice invoice) => base.UpdateAsync(invoice);

    public async Task<decimal> SumOpenByAccountAsync(string accountId, string userId)
    {
        var f = Builders<CreditCardInvoice>.Filter.Eq(x => x.AccountId, accountId)
              & Builders<CreditCardInvoice>.Filter.Eq(x => x.UserId, userId)
              & Builders<CreditCardInvoice>.Filter.Ne(x => x.Status, CreditCardInvoiceStatus.Paid);
        var result = await Collection.Aggregate()
            .Match(f)
            .Group(_ => 1, g => new { Total = g.Sum(x => x.TotalAmount) })
            .FirstOrDefaultAsync();
        return result?.Total ?? 0m;
    }
}
