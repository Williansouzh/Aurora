using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Dashboard.Common;
using MediatR;

namespace Aurora.Application.Features.Dashboard.CategoryExpenses;

public record GetCategoryExpensesQuery(string UserId, int Month, int Year) : IRequest<List<CategoryExpenseDto>>;

public class GetCategoryExpensesHandler(
    ITransactionRepository transactions,
    ICategoryRepository categories,
    ICacheService cache) : IRequestHandler<GetCategoryExpensesQuery, List<CategoryExpenseDto>>
{
    public async Task<List<CategoryExpenseDto>> Handle(GetCategoryExpensesQuery query, CancellationToken ct)
    {
        var key = CacheKeys.CategoryExpenses(query.UserId, query.Month, query.Year);
        var cached = await cache.GetAsync<List<CategoryExpenseDto>>(key, ct);
        if (cached is not null) return cached;

        var grouped = await transactions.CategoryExpenseAsync(query.UserId, query.Month, query.Year);
        var lookup = (await categories.GetByUserAsync(query.UserId)).ToDictionary(x => x.Id, x => x);
        var total = grouped.Sum(x => x.Total);

        var result = grouped
            .Select(x =>
            {
                lookup.TryGetValue(x.CategoryId, out var category);
                var percentage = total == 0 ? 0 : Math.Round((x.Total / total) * 100, 2);
                return new CategoryExpenseDto(
                    x.CategoryId,
                    category?.Name ?? "N/A",
                    category?.Color ?? "#94a3b8",
                    x.Total,
                    percentage);
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        await cache.SetAsync(key, result, TimeSpan.FromMinutes(5), ct);
        return result;
    }
}
