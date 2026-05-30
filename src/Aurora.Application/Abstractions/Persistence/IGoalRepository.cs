using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Abstractions.Persistence;

public interface IGoalRepository : IRepository<Goal>
{
    Task<List<Goal>> GetByStatusAsync(string userId, GoalStatus? status);
}
