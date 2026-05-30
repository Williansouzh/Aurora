using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IHabitRepository : IRepository<Habit>
{
    Task<List<Habit>> GetActiveAsync(string userId);
}
