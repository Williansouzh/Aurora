using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IPlanRepository
{
    Task<List<Plan>> GetAllAsync(CancellationToken ct = default);
    Task<Plan?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Plan?> GetByKeyAsync(string key, CancellationToken ct = default);
    Task AddAsync(Plan plan, CancellationToken ct = default);
    Task UpdateAsync(Plan plan, CancellationToken ct = default);
    Task UpsertAsync(Plan plan, CancellationToken ct = default);
}
