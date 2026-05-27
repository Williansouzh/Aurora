using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Dashboard.Common;
using MediatR;

namespace Aurora.Application.Features.Dashboard.CashFlow;

public record GetCashFlowQuery(string UserId, int Year) : IRequest<List<CashFlowItemDto>>;

public class GetCashFlowHandler(ITransactionRepository transactions) : IRequestHandler<GetCashFlowQuery, List<CashFlowItemDto>>
{
    public async Task<List<CashFlowItemDto>> Handle(GetCashFlowQuery query, CancellationToken ct) =>
        (await transactions.CashFlowAsync(query.UserId, query.Year))
            .Select(x => new CashFlowItemDto(x.Month, x.Income, x.Expense, x.Income - x.Expense))
            .ToList();
}
