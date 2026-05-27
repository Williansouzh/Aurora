using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class AccountRepository(MongoContext context)
    : MongoRepositoryBase<Account>(context.Accounts), IAccountRepository
{
    public Task<List<Account>> GetByUserAsync(string userId) =>
        base.GetByUserAsync(userId);

    public Task<Account?> GetByIdAsync(string id, string userId) =>
        base.GetByIdAsync(id, userId);

    public Task AddAsync(Account account) => base.AddAsync(account);

    public Task UpdateAsync(Account account) => base.UpdateAsync(account);

    public Task DeleteAsync(string id, string userId) => base.DeleteAsync(id, userId);

    public async Task<decimal> GetTotalBalanceAsync(string userId)
    {
        var rows = await Collection
            .Find(x => x.UserId == userId && !x.IsArchived && x.Type != AccountType.CreditCard)
            .ToListAsync();
        return rows.Sum(x => x.CurrentBalance);
    }
}
