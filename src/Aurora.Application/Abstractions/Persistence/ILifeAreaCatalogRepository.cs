using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface ILifeAreaCatalogRepository
{
    Task<List<LifeAreaCatalogItem>> GetAllAsync(CancellationToken ct = default);
    Task UpsertAsync(LifeAreaCatalogItem area, CancellationToken ct = default);
}
