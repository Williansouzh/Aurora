using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IHabitCheckInRepository : IRepository<HabitCheckIn>
{
    Task<HabitCheckIn?> GetByHabitAndDateAsync(string habitId, string userId, DateTime date);
    Task<List<HabitCheckIn>> GetByHabitAsync(string habitId, string userId, DateTime from, DateTime to);
    Task<List<HabitCheckIn>> GetByUserAndDateAsync(string userId, DateTime date);
}
