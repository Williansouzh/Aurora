using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface ICategoryRepository
{
    Task<List<Category>> GetByUserAsync(string userId);
    Task<Category?> GetByIdAsync(string id, string userId);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(string id, string userId);
    Task SeedDefaultsAsync(string userId);
}
