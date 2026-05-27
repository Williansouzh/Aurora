using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class BudgetRepository(MongoContext context)
    : MongoRepositoryBase<Budget>(context.Budgets), IBudgetRepository
{
    public Task<List<Budget>> GetByMonthAsync(string userId, int month, int year) =>
        Collection.Find(x => x.UserId == userId && x.Month == month && x.Year == year).ToListAsync();

    public Task<Budget?> GetByCategoryPeriodAsync(string userId, string categoryId, int month, int year) =>
        Collection.Find(x => x.UserId == userId && x.CategoryId == categoryId && x.Month == month && x.Year == year)
            .FirstOrDefaultAsync()!;

    public Task<Budget?> GetByIdAsync(string id, string userId) => base.GetByIdAsync(id, userId);

    public Task AddAsync(Budget budget) => base.AddAsync(budget);

    public Task UpdateAsync(Budget budget) => base.UpdateAsync(budget);

    public Task DeleteAsync(string id, string userId) => base.DeleteAsync(id, userId);
}
