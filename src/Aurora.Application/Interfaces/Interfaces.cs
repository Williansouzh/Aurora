using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
namespace Aurora.Application.Interfaces;

public interface IUserContext { string UserId { get; } }
public interface IJwtTokenService { string Generate(User user); int ExpiresInSeconds { get; } }
public interface IPasswordHasher { string Hash(string password); bool Verify(string password, string hash); }
public interface ICacheService { Task<T?> GetAsync<T>(string key, CancellationToken ct=default); Task SetAsync<T>(string key,T value, TimeSpan ttl, CancellationToken ct=default); Task RemoveByPrefixAsync(string prefix, CancellationToken ct=default); }

public interface IRefreshTokenRepository { Task AddAsync(RefreshToken token); Task<RefreshToken?> GetByHashAsync(string hash); Task RevokeAsync(string id); Task RevokeAllByUserAsync(string userId); }
public interface IRateLimiter { Task<bool> IsAllowedAsync(string key, int maxRequests, TimeSpan window); }
public interface IUserRepository { Task<User?> GetByEmailAsync(string email); Task<User?> GetByIdAsync(string id); Task AddAsync(User user); Task UpdateAsync(User user); }
public interface IAccountRepository { Task<List<Account>> GetByUserAsync(string userId); Task<Account?> GetByIdAsync(string id,string userId); Task AddAsync(Account account); Task UpdateAsync(Account account); Task DeleteAsync(string id,string userId); Task<decimal> GetTotalBalanceAsync(string userId); }
public interface ICategoryRepository { Task<List<Category>> GetByUserAsync(string userId); Task<Category?> GetByIdAsync(string id,string userId); Task AddAsync(Category category); Task UpdateAsync(Category category); Task DeleteAsync(string id,string userId); Task SeedDefaultsAsync(string userId); }
public record TransactionFilter(string UserId,int? Month,int? Year,TransactionType? Type,TransactionStatus? Status,string? CategoryId,string? AccountId,string? Search=null,DateTime? DateFrom=null,DateTime? DateTo=null,List<string>? AccountIds=null,List<string>? CategoryIds=null,decimal? MinAmount=null,decimal? MaxAmount=null);

public interface ITransactionRepository {
 Task<List<Transaction>> GetByFilterAsync(string userId,int? month,int? year,TransactionType? type,TransactionStatus? status,string? categoryId,string? accountId);
 Task<(List<Transaction> Items,int TotalCount)> GetPagedAsync(TransactionFilter filter,int page,int pageSize);
 Task<Transaction?> GetByIdAsync(string id,string userId);
 Task AddAsync(Transaction tx); Task UpdateAsync(Transaction tx); Task DeleteAsync(string id,string userId);
 Task<bool> ExistsByAccountIdAsync(string accountId,string userId); Task<bool> ExistsByCategoryIdAsync(string categoryId,string userId);
 Task<decimal> SumAsync(string userId,int month,int year,TransactionType type,TransactionStatus status);
 Task<int> CountAsync(string userId,int month,int year,TransactionStatus status);
 Task<List<Transaction>> RecentAsync(string userId,int limit=5);
 Task<List<(string CategoryId, decimal Total)>> CategoryExpenseAsync(string userId,int month,int year);
 Task<List<(int Month, decimal Income, decimal Expense)>> CashFlowAsync(string userId,int year);
 Task<List<Transaction>> UpcomingDueAsync(string userId,int days);
 Task<List<Transaction>> GetByInvoiceIdAsync(string invoiceId,string userId);
 Task<List<Transaction>> GetByRecurrenceGroupAsync(Guid groupId,string userId);
}
public interface IFinancingRepository { Task<List<Financing>> GetByUserAsync(string userId); Task<Financing?> GetByIdAsync(string id,string userId); Task AddAsync(Financing financing); Task UpdateAsync(Financing financing); Task DeleteAsync(string id,string userId); }
public interface ICreditCardInvoiceRepository { Task<List<CreditCardInvoice>> GetByAccountAsync(string accountId,string userId); Task<CreditCardInvoice?> GetByPeriodAsync(string accountId,string userId,int month,int year); Task<CreditCardInvoice?> GetByIdAsync(string id,string userId); Task AddAsync(CreditCardInvoice invoice); Task UpdateAsync(CreditCardInvoice invoice); Task<decimal> SumOpenByAccountAsync(string accountId,string userId); }
public interface IBudgetRepository { Task<List<Budget>> GetByMonthAsync(string userId,int month,int year); Task<Budget?> GetByCategoryPeriodAsync(string userId,string categoryId,int month,int year); Task<Budget?> GetByIdAsync(string id,string userId); Task AddAsync(Budget budget); Task UpdateAsync(Budget budget); Task DeleteAsync(string id,string userId); }
public interface ITransferRepository { Task<List<Transfer>> GetByFilterAsync(string userId,int? month,int? year); Task<Transfer?> GetByIdAsync(string id,string userId); Task AddAsync(Transfer transfer); Task DeleteAsync(string id,string userId); }
