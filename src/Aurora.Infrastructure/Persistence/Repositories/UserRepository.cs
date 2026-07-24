using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class UserRepository(MongoContext context) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email) =>
        context.Users.Find(x => x.Email == email).FirstOrDefaultAsync()!;

    public Task<User?> GetByEmailHashAsync(string emailHash) =>
        context.Users.Find(x => x.EmailHash == emailHash).FirstOrDefaultAsync()!;

    public Task<User?> GetByIdAsync(string id) =>
        context.Users.Find(x => x.Id == id).FirstOrDefaultAsync()!;

    public Task AddAsync(User user) => context.Users.InsertOneAsync(user);

    public Task UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        return context.Users.ReplaceOneAsync(x => x.Id == user.Id, user);
    }

    public Task<List<User>> GetAllAsync() =>
        context.Users.Find(x => x.DeletedAt == null).ToListAsync();

    public async Task<(List<User> Items, long Total)> GetPagedAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var builder = Builders<User>.Filter;
        var filter = builder.Eq(x => x.DeletedAt, null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = new MongoDB.Bson.BsonRegularExpression(
                System.Text.RegularExpressions.Regex.Escape(search.Trim()), "i");
            filter &= builder.Or(
                builder.Regex(x => x.Name, term),
                builder.Regex(x => x.Email, term));
        }

        var total = await context.Users.CountDocumentsAsync(filter, cancellationToken: ct);
        var items = await context.Users.Find(filter)
            .SortBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<List<User>> GetHabitReminderCandidatesAsync(int hour) =>
        context.Users.Find(x =>
            x.DeletedAt == null &&
            x.Notifications.HabitReminderEnabled &&
            x.Notifications.HabitReminderHour == hour).ToListAsync();

    public Task<List<User>> GetWeeklyPlanningReminderCandidatesAsync(int hour) =>
        context.Users.Find(x =>
            x.DeletedAt == null &&
            x.Notifications.WeeklyPlanningReminderEnabled &&
            x.Notifications.WeeklyPlanningReminderHour == hour).ToListAsync();
}
