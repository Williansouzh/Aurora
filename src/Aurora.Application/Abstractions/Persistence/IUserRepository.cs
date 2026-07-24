using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByEmailHashAsync(string emailHash);
    Task<User?> GetByIdAsync(string id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<List<User>> GetAllAsync();

    /// <summary>Page of non-deleted users optionally filtered by name/email, ordered by name, with the total count.</summary>
    Task<(List<User> Items, long Total)> GetPagedAsync(string? search, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Users who have habit reminders enabled for the given hour of day (UTC).</summary>
    Task<List<User>> GetHabitReminderCandidatesAsync(int hour);

    /// <summary>Users who have weekly-planning reminders enabled for the given hour of day (UTC).</summary>
    Task<List<User>> GetWeeklyPlanningReminderCandidatesAsync(int hour);
}
