using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IModuleCatalogRepository
{
    Task<List<ModuleCatalogItem>> GetAllAsync(CancellationToken ct = default);
    Task<ModuleCatalogItem?> GetByKeyAsync(string key, CancellationToken ct = default);
    Task UpsertAsync(ModuleCatalogItem module, CancellationToken ct = default);
}
