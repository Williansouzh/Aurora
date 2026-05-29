using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Dashboard.Common;
using MediatR;

namespace Aurora.Application.Features.Dashboard.CashFlow;

public record GetCashFlowQuery(string UserId, int Year) : IRequest<List<CashFlowItemDto>>;

public class GetCashFlowHandler(ITransactionRepository transactions, ICacheService cache)
    : IRequestHandler<GetCashFlowQuery, List<CashFlowItemDto>>
{
    public async Task<List<CashFlowItemDto>> Handle(GetCashFlowQuery query, CancellationToken ct)
    {
        var key = CacheKeys.CashFlow(query.UserId, query.Year);
        var cached = await cache.GetAsync<List<CashFlowItemDto>>(key, ct);
        if (cached is not null) return cached;

        var result = (await transactions.CashFlowAsync(query.UserId, query.Year))
            .Select(x => new CashFlowItemDto(x.Month, x.Income, x.Expense, x.Income - x.Expense))
            .ToList();

        await cache.SetAsync(key, result, TimeSpan.FromMinutes(5), ct);
        return result;
    }
}
