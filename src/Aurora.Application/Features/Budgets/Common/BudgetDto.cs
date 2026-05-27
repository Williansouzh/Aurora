using Aurora.Domain.Entities;

namespace Aurora.Application.Features.Budgets.Common;

public record BudgetDto(
    string? Id,
    string CategoryId,
    string CategoryName,
    string CategoryIcon,
    string CategoryColor,
    int Month,
    int Year,
    decimal? LimitAmount,
    decimal SpentAmount,
    decimal UsagePercentage,
    bool HasBudget);

public static class BudgetMapper
{
    public static BudgetDto ToBudgetDto(Category category, Budget? budget, decimal spent, int month, int year)
    {
        var limit = budget?.LimitAmount;
        var usage = limit.HasValue && limit.Value > 0
            ? Math.Round((spent / limit.Value) * 100, 2)
            : 0;

        return new BudgetDto(
            budget?.Id,
            category.Id,
            category.Name,
            category.Icon,
            category.Color,
            month,
            year,
            limit,
            spent,
            usage,
            budget is not null);
    }
}
