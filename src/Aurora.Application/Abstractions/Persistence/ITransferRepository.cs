using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface ITransferRepository
{
    Task<List<Transfer>> GetByFilterAsync(string userId, int? month, int? year);
    Task<Transfer?> GetByIdAsync(string id, string userId);
    Task AddAsync(Transfer transfer);
    Task DeleteAsync(string id, string userId);
}
