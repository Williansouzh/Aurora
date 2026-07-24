using System.Text.RegularExpressions;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class TransactionRepository(MongoContext context, UnitOfWork.MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<Transaction>(context.Transactions, unitOfWork), ITransactionRepository
{
    // Transactions are a financial ledger: deletes are soft (DeletedAt is set) so the record is
    // retained for audit, and every read below excludes soft-deleted rows.
    private static readonly FilterDefinition<Transaction> NotDeleted =
        Builders<Transaction>.Filter.Eq(x => x.DeletedAt, null);

    public async Task<Transaction?> GetByIdAsync(string id, string userId)
    {
        var tx = await base.GetByIdAsync(id, userId);
        return tx?.DeletedAt is null ? tx : null;
    }

    public Task AddAsync(Transaction tx) => base.AddAsync(tx);

    public Task UpdateAsync(Transaction tx) => base.UpdateAsync(tx);

    public async Task DeleteAsync(string id, string userId)
    {
        var tx = await GetByIdAsync(id, userId);
        if (tx is null) return;
        tx.DeletedAt = DateTime.UtcNow;
        await base.UpdateAsync(tx);
    }

    public async Task<List<Transaction>> GetByFilterAsync(
        string userId,
        int? month,
        int? year,
        TransactionType? type,
        TransactionStatus? status,
        string? categoryId,
        string? accountId)
    {
        var filter = BuildFilter(new TransactionFilter(userId, month, year, type, status, categoryId, accountId));
        return await Collection.Find(filter).SortByDescending(x => x.Date).ToListAsync();
    }

    public async Task<(List<Transaction> Items, int TotalCount)> GetPagedAsync(TransactionFilter filter, int page, int pageSize)
    {
        var mongoFilter = BuildFilter(filter);
        var totalLong = await Collection.CountDocumentsAsync(mongoFilter);
        var skip = (page - 1) * pageSize;
        var items = await Collection.Find(mongoFilter)
            .SortByDescending(x => x.Date)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();
        return (items, (int)totalLong);
    }

    public Task<bool> ExistsByAccountIdAsync(string accountId, string userId) =>
        Collection.Find(x => x.AccountId == accountId && x.UserId == userId && x.DeletedAt == null).AnyAsync();

    public Task<bool> ExistsByCategoryIdAsync(string categoryId, string userId) =>
        Collection.Find(x => x.CategoryId == categoryId && x.UserId == userId && x.DeletedAt == null).AnyAsync();

    public async Task<decimal> SumAsync(string userId, int month, int year, TransactionType type, TransactionStatus status)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);
        var f = Builders<Transaction>.Filter.Eq(x => x.UserId, userId)
              & Builders<Transaction>.Filter.Gte(x => x.Date, start)
              & Builders<Transaction>.Filter.Lt(x => x.Date, end)
              & Builders<Transaction>.Filter.Eq(x => x.Type, type)
              & Builders<Transaction>.Filter.Eq(x => x.Status, status)
              & NotDeleted;
        var result = await Collection.Aggregate()
            .Match(f)
            .Group(_ => 1, g => new { Total = g.Sum(x => x.Amount) })
            .FirstOrDefaultAsync();
        return result?.Total ?? 0m;
    }

    public async Task<int> CountAsync(string userId, int month, int year, TransactionStatus status)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);
        var f = Builders<Transaction>.Filter.Eq(x => x.UserId, userId)
              & Builders<Transaction>.Filter.Gte(x => x.Date, start)
              & Builders<Transaction>.Filter.Lt(x => x.Date, end)
              & Builders<Transaction>.Filter.Eq(x => x.Status, status)
              & NotDeleted;
        return (int)await Collection.CountDocumentsAsync(f);
    }

    public Task<List<Transaction>> RecentAsync(string userId, int limit = 5) =>
        Collection.Find(x => x.UserId == userId && x.DeletedAt == null)
            .SortByDescending(x => x.Date)
            .Limit(limit)
            .ToListAsync();

    public async Task<List<(string CategoryId, decimal Total)>> CategoryExpenseAsync(string userId, int month, int year)
    {
        var rows = await GetByFilterAsync(userId, month, year, TransactionType.Expense, TransactionStatus.Paid, null, null);
        return rows.GroupBy(x => x.CategoryId).Select(g => (g.Key, g.Sum(x => x.Amount))).ToList();
    }

    public async Task<List<(int Month, decimal Income, decimal Expense)>> CashFlowAsync(string userId, int year)
    {
        var list = new List<(int, decimal, decimal)>();
        for (var m = 1; m <= 12; m++)
        {
            var income = await SumAsync(userId, m, year, TransactionType.Income, TransactionStatus.Paid);
            var expense = await SumAsync(userId, m, year, TransactionType.Expense, TransactionStatus.Paid);
            list.Add((m, income, expense));
        }
        return list;
    }

    public Task<List<Transaction>> UpcomingDueAsync(string userId, int days)
    {
        var today = DateTime.UtcNow.Date;
        var end = today.AddDays(Math.Max(1, days));
        var statuses = new[] { TransactionStatus.Pending, TransactionStatus.Overdue };
        var f = Builders<Transaction>.Filter.Eq(x => x.UserId, userId)
              & Builders<Transaction>.Filter.Ne(x => x.DueDate, null)
              & Builders<Transaction>.Filter.Lte(x => x.DueDate, end)
              & Builders<Transaction>.Filter.In(x => x.Status, statuses)
              & NotDeleted;
        return Collection.Find(f).SortBy(x => x.DueDate).ToListAsync();
    }

    public Task<List<Transaction>> GetByInvoiceIdAsync(string invoiceId, string userId) =>
        Collection.Find(x => x.UserId == userId && x.CreditCardInvoiceId == invoiceId && x.DeletedAt == null)
            .SortByDescending(x => x.Date)
            .ToListAsync();

    public Task<List<Transaction>> GetByRecurrenceGroupAsync(Guid groupId, string userId) =>
        Collection.Find(x => x.UserId == userId && x.RecurrenceGroupId == groupId && x.DeletedAt == null)
            .SortBy(x => x.Date)
            .ToListAsync();

    private static FilterDefinition<Transaction> BuildFilter(TransactionFilter q)
    {
        var b = Builders<Transaction>.Filter;
        var f = b.Eq(x => x.UserId, q.UserId) & NotDeleted;

        if (q.DateFrom.HasValue || q.DateTo.HasValue)
        {
            if (q.DateFrom.HasValue) f &= b.Gte(x => x.Date, q.DateFrom.Value);
            if (q.DateTo.HasValue) f &= b.Lt(x => x.Date, q.DateTo.Value.Date.AddDays(1));
        }
        else if (q.Month.HasValue && q.Year.HasValue)
        {
            var start = new DateTime(q.Year.Value, q.Month.Value, 1);
            var end = start.AddMonths(1);
            f &= b.Gte(x => x.Date, start) & b.Lt(x => x.Date, end);
        }

        if (q.Type.HasValue) f &= b.Eq(x => x.Type, q.Type);
        if (q.Status.HasValue) f &= b.Eq(x => x.Status, q.Status);

        var accountIds = NormalizeIds(q.AccountIds, q.AccountId);
        if (accountIds.Count > 0) f &= b.In(x => x.AccountId, accountIds);

        var categoryIds = NormalizeIds(q.CategoryIds, q.CategoryId);
        if (categoryIds.Count > 0) f &= b.In(x => x.CategoryId, categoryIds);

        if (q.MinAmount.HasValue) f &= b.Gte(x => x.Amount, q.MinAmount.Value);
        if (q.MaxAmount.HasValue) f &= b.Lte(x => x.Amount, q.MaxAmount.Value);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var regex = new BsonRegularExpression(Regex.Escape(q.Search.Trim()), "i");
            f &= b.Regex(x => x.Description, regex);
        }

        return f;
    }

    private static List<string> NormalizeIds(List<string>? many, string? single)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        if (many is not null)
        {
            foreach (var id in many)
            {
                if (!string.IsNullOrWhiteSpace(id)) set.Add(id.Trim());
            }
        }
        if (!string.IsNullOrWhiteSpace(single)) set.Add(single.Trim());
        return [.. set];
    }
}
