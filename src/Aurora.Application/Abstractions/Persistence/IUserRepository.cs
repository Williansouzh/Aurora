using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByEmailHashAsync(string emailHash);
    Task<User?> GetByIdAsync(string id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}
