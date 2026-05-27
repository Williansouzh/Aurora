using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Transfers.Common;
using MediatR;

namespace Aurora.Application.Features.Transfers.GetAll;

public record GetTransfersQuery(string UserId, int? Month, int? Year) : IRequest<List<TransferDto>>;

public class GetTransfersHandler(ITransferRepository transfers) : IRequestHandler<GetTransfersQuery, List<TransferDto>>
{
    public async Task<List<TransferDto>> Handle(GetTransfersQuery query, CancellationToken ct) =>
        (await transfers.GetByFilterAsync(query.UserId, query.Month, query.Year))
            .Select(x => x.ToDto())
            .ToList();
}
