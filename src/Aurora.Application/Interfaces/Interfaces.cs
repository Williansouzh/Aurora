using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
namespace Aurora.Application.Interfaces;

public interface IUserContext { string UserId { get; } }
public interface IJwtTokenService { string Generate(User user); }
public interface IPasswordHasher { string Hash(string password); bool Verify(string password, string hash); }
public interface ICacheService { Task<T?> GetAsync<T>(string key, CancellationToken ct=default); Task SetAsync<T>(string key,T value, TimeSpan ttl, CancellationToken ct=default); Task RemoveByPrefixAsync(string prefix, CancellationToken ct=default); }

public interface IUserRepository { Task<User?> GetByEmailAsync(string email); Task<User?> GetByIdAsync(string id); Task AddAsync(User user); }
public interface IAccountRepository { Task<List<Account>> GetByUserAsync(string userId); Task<Account?> GetByIdAsync(string id,string userId); Task AddAsync(Account account); Task UpdateAsync(Account account); Task DeleteAsync(string id,string userId); Task<decimal> GetTotalBalanceAsync(string userId); }
public interface ICategoryRepository { Task<List<Category>> GetByUserAsync(string userId); Task<Category?> GetByIdAsync(string id,string userId); Task AddAsync(Category category); Task UpdateAsync(Category category); Task DeleteAsync(string id,string userId); Task SeedDefaultsAsync(string userId); }
public interface ITransactionRepository {
 Task<List<Transaction>> GetByFilterAsync(string userId,int? month,int? year,TransactionType? type,TransactionStatus? status,string? categoryId,string? accountId);
 Task<Transaction?> GetByIdAsync(string id,string userId);
 Task AddAsync(Transaction tx); Task UpdateAsync(Transaction tx); Task DeleteAsync(string id,string userId);
 Task<bool> ExistsByAccountIdAsync(string accountId,string userId); Task<bool> ExistsByCategoryIdAsync(string categoryId,string userId);
 Task<decimal> SumAsync(string userId,int month,int year,TransactionType type,TransactionStatus status);
 Task<int> CountAsync(string userId,int month,int year,TransactionStatus status);
 Task<List<Transaction>> RecentAsync(string userId,int limit=5);
 Task<List<(string CategoryId, decimal Total)>> CategoryExpenseAsync(string userId,int month,int year);
 Task<List<(int Month, decimal Income, decimal Expense)>> CashFlowAsync(string userId,int year);
}
