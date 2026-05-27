using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class TransferRepository(MongoContext context)
    : MongoRepositoryBase<Transfer>(context.Transfers), ITransferRepository
{
    public Task<Transfer?> GetByIdAsync(string id, string userId) => base.GetByIdAsync(id, userId);

    public Task AddAsync(Transfer transfer) => base.AddAsync(transfer);

    public Task DeleteAsync(string id, string userId) => base.DeleteAsync(id, userId);

    public Task<List<Transfer>> GetByFilterAsync(string userId, int? month, int? year)
    {
        var b = Builders<Transfer>.Filter;
        var f = b.Eq(x => x.UserId, userId);

        if (month.HasValue && year.HasValue)
        {
            var start = new DateTime(year.Value, month.Value, 1);
            var end = start.AddMonths(1);
            f &= b.Gte(x => x.Date, start) & b.Lt(x => x.Date, end);
        }

        return Collection.Find(f).SortByDescending(x => x.Date).ToListAsync();
    }
}
