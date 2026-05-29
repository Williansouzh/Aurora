using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Budgets.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Budgets.GetAll;

public record GetBudgetsQuery(string UserId, int Month, int Year) : IRequest<List<BudgetDto>>;

public class GetBudgetsHandler(
    IBudgetRepository budgets,
    ICategoryRepository categories,
    ITransactionRepository transactions,
    ICacheService cache) : IRequestHandler<GetBudgetsQuery, List<BudgetDto>>
{
    public async Task<List<BudgetDto>> Handle(GetBudgetsQuery query, CancellationToken ct)
    {
        var key = CacheKeys.Budgets(query.UserId, query.Month, query.Year);
        var cached = await cache.GetAsync<List<BudgetDto>>(key, ct);
        if (cached is not null) return cached;

        var expenseCategories = (await categories.GetByUserAsync(query.UserId))
            .Where(x => x.Type == CategoryType.Expense)
            .OrderBy(x => x.Name)
            .ToList();

        var budgetsByCategory = (await budgets.GetByMonthAsync(query.UserId, query.Month, query.Year))
            .ToDictionary(x => x.CategoryId, x => x);

        var spentByCategory = (await transactions.CategoryExpenseAsync(query.UserId, query.Month, query.Year))
            .ToDictionary(x => x.CategoryId, x => x.Total);

        var result = expenseCategories
            .Select(category => BudgetMapper.ToBudgetDto(
                category,
                budgetsByCategory.GetValueOrDefault(category.Id),
                spentByCategory.GetValueOrDefault(category.Id),
                query.Month,
                query.Year))
            .OrderByDescending(x => x.HasBudget && x.LimitAmount > 0 ? x.UsagePercentage : -1)
            .ThenBy(x => x.CategoryName)
            .ToList();

        await cache.SetAsync(key, result, TimeSpan.FromMinutes(5), ct);
        return result;
    }
}
