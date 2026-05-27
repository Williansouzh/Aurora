using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Financings.Common;
using MediatR;

namespace Aurora.Application.Features.Financings.GetAll;

public record GetFinancingsQuery(string UserId) : IRequest<List<FinancingDto>>;

public class GetFinancingsHandler(IFinancingRepository financings) : IRequestHandler<GetFinancingsQuery, List<FinancingDto>>
{
    public async Task<List<FinancingDto>> Handle(GetFinancingsQuery query, CancellationToken ct) =>
        (await financings.GetByUserAsync(query.UserId)).Select(x => x.ToDto()).ToList();
}
