using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Goals.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Goals.SyncFinancial;

public record SyncFinancialGoalCommand(string UserId, string GoalId, int Month, int Year) : IRequest<GoalDto?>;

public record SyncAllFinancialGoalsCommand(string UserId, int Month, int Year) : IRequest;

public class SyncFinancialGoalHandler(IGoalRepository goalRepo, ITransactionRepository txRepo)
    : IRequestHandler<SyncFinancialGoalCommand, GoalDto?>
{
    public async Task<GoalDto?> Handle(SyncFinancialGoalCommand cmd, CancellationToken ct)
    {
        var goal = await goalRepo.GetByIdAsync(cmd.GoalId, cmd.UserId, ct);
        if (goal is null || goal.LinkedCategoryId is null) return null;

        var byCategory = await txRepo.CategoryExpenseAsync(cmd.UserId, cmd.Month, cmd.Year);
        var spent = byCategory
            .FirstOrDefault(x => x.CategoryId == goal.LinkedCategoryId)
            .Total;

        goal.CurrentValue = spent;
        goal.UpdatedAt = DateTime.UtcNow;
        await goalRepo.UpdateAsync(goal, ct);
        return goal.ToDto();
    }
}

public class SyncAllFinancialGoalsHandler(IGoalRepository goalRepo, ITransactionRepository txRepo)
    : IRequestHandler<SyncAllFinancialGoalsCommand>
{
    public async Task Handle(SyncAllFinancialGoalsCommand cmd, CancellationToken ct)
    {
        var goals = await goalRepo.GetByStatusAsync(cmd.UserId, GoalStatus.Active);
        var financialGoals = goals.Where(g => g.Area == LifeArea.Money && g.LinkedCategoryId is not null).ToList();
        if (financialGoals.Count == 0) return;

        var byCategory = await txRepo.CategoryExpenseAsync(cmd.UserId, cmd.Month, cmd.Year);
        var lookup = byCategory.ToDictionary(x => x.CategoryId, x => x.Total);

        foreach (var goal in financialGoals)
        {
            goal.CurrentValue = lookup.GetValueOrDefault(goal.LinkedCategoryId!, 0m);
            goal.UpdatedAt = DateTime.UtcNow;
            await goalRepo.UpdateAsync(goal, ct);
        }
    }
}
