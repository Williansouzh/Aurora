using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class FinancingRepository(MongoContext context)
    : MongoRepositoryBase<Financing>(context.Financings), IFinancingRepository
{
    public Task<List<Financing>> GetByUserAsync(string userId) =>
        Collection.Find(x => x.UserId == userId).SortBy(x => x.Name).ToListAsync();

    public Task<Financing?> GetByIdAsync(string id, string userId) => base.GetByIdAsync(id, userId);

    public Task AddAsync(Financing financing) => base.AddAsync(financing);

    public Task UpdateAsync(Financing financing) => base.UpdateAsync(financing);

    public Task DeleteAsync(string id, string userId) => base.DeleteAsync(id, userId);
}
