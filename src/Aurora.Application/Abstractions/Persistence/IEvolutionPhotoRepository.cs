using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IEvolutionPhotoRepository : IRepository<EvolutionPhoto>
{
    Task<List<EvolutionPhoto>> GetByAlbumAsync(string albumId, string userId);
    Task<List<EvolutionPhoto>> GetRecentAsync(string userId, int limit = 6);
    Task DeleteByAlbumAsync(string albumId, string userId);
}
