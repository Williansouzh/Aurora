using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class EvolutionPhotoRepository(MongoContext context, MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<EvolutionPhoto>(context.EvolutionPhotos, unitOfWork), IEvolutionPhotoRepository
{
    public Task<List<EvolutionPhoto>> GetByAlbumAsync(string albumId, string userId) =>
        Collection.Find(x => x.AlbumId == albumId && x.UserId == userId)
            .SortByDescending(x => x.Date)
            .ToListAsync();

    public Task<List<EvolutionPhoto>> GetRecentAsync(string userId, int limit = 6) =>
        Collection.Find(x => x.UserId == userId)
            .SortByDescending(x => x.Date)
            .Limit(limit)
            .ToListAsync();

    public Task DeleteByAlbumAsync(string albumId, string userId) =>
        Collection.DeleteManyAsync(x => x.AlbumId == albumId && x.UserId == userId);
}
