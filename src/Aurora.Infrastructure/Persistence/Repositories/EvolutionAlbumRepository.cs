using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class EvolutionAlbumRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<EvolutionAlbum>(context.EvolutionAlbums, unitOfWork), IEvolutionAlbumRepository
{
    public Task<List<EvolutionAlbum>> GetAllAsync(string userId) =>
        Collection.Find(x => x.UserId == userId).SortBy(x => x.Title).ToListAsync();
}
