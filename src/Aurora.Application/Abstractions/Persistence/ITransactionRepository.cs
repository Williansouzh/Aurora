using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Abstractions.Persistence;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetByFilterAsync(
        string userId,
        int? month,
        int? year,
        TransactionType? type,
        TransactionStatus? status,
        string? categoryId,
        string? accountId);

    Task<(List<Transaction> Items, int TotalCount)> GetPagedAsync(TransactionFilter filter, int page, int pageSize);
    Task<Transaction?> GetByIdAsync(string id, string userId);
    Task AddAsync(Transaction tx);
    Task UpdateAsync(Transaction tx);
    Task DeleteAsync(string id, string userId);

    Task<bool> ExistsByAccountIdAsync(string accountId, string userId);
    Task<bool> ExistsByCategoryIdAsync(string categoryId, string userId);

    Task<decimal> SumAsync(string userId, int month, int year, TransactionType type, TransactionStatus status);
    Task<int> CountAsync(string userId, int month, int year, TransactionStatus status);
    Task<List<Transaction>> RecentAsync(string userId, int limit = 5);
    Task<List<(string CategoryId, decimal Total)>> CategoryExpenseAsync(string userId, int month, int year);
    Task<List<(int Month, decimal Income, decimal Expense)>> CashFlowAsync(string userId, int year);
    Task<List<Transaction>> UpcomingDueAsync(string userId, int days);
    Task<List<Transaction>> GetByInvoiceIdAsync(string invoiceId, string userId);
    Task<List<Transaction>> GetByRecurrenceGroupAsync(Guid groupId, string userId);
}
