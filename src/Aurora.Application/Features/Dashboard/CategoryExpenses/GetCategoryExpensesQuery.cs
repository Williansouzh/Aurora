using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Dashboard.Common;
using MediatR;

namespace Aurora.Application.Features.Dashboard.CategoryExpenses;

public record GetCategoryExpensesQuery(string UserId, int Month, int Year) : IRequest<List<CategoryExpenseDto>>;

public class GetCategoryExpensesHandler(
    ITransactionRepository transactions,
    ICategoryRepository categories) : IRequestHandler<GetCategoryExpensesQuery, List<CategoryExpenseDto>>
{
    public async Task<List<CategoryExpenseDto>> Handle(GetCategoryExpensesQuery query, CancellationToken ct)
    {
        var grouped = await transactions.CategoryExpenseAsync(query.UserId, query.Month, query.Year);
        var lookup = (await categories.GetByUserAsync(query.UserId)).ToDictionary(x => x.Id, x => x);
        var total = grouped.Sum(x => x.Total);

        return grouped
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
    }
}
