using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IEvolutionAlbumRepository : IRepository<EvolutionAlbum>
{
    Task<List<EvolutionAlbum>> GetAllAsync(string userId);
}
