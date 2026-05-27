using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IFinancingRepository
{
    Task<List<Financing>> GetByUserAsync(string userId);
    Task<Financing?> GetByIdAsync(string id, string userId);
    Task AddAsync(Financing financing);
    Task UpdateAsync(Financing financing);
    Task DeleteAsync(string id, string userId);
}
