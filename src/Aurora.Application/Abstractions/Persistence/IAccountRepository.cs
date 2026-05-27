using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IAccountRepository
{
    Task<List<Account>> GetByUserAsync(string userId);
    Task<Account?> GetByIdAsync(string id, string userId);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task DeleteAsync(string id, string userId);
    Task<decimal> GetTotalBalanceAsync(string userId);
}
