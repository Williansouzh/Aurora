using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IBudgetRepository
{
    Task<List<Budget>> GetByMonthAsync(string userId, int month, int year);
    Task<Budget?> GetByCategoryPeriodAsync(string userId, string categoryId, int month, int year);
    Task<Budget?> GetByIdAsync(string id, string userId);
    Task AddAsync(Budget budget);
    Task UpdateAsync(Budget budget);
    Task DeleteAsync(string id, string userId);
}
