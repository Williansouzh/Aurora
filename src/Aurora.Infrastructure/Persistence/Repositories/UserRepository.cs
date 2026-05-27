using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class UserRepository(MongoContext context) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email) =>
        context.Users.Find(x => x.Email == email).FirstOrDefaultAsync()!;

    public Task<User?> GetByIdAsync(string id) =>
        context.Users.Find(x => x.Id == id).FirstOrDefaultAsync()!;

    public Task AddAsync(User user) => context.Users.InsertOneAsync(user);

    public Task UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        return context.Users.ReplaceOneAsync(x => x.Id == user.Id, user);
    }
}
