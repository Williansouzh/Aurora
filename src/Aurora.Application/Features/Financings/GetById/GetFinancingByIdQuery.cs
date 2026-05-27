using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Financings.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Financings.GetById;

public record GetFinancingByIdQuery(string UserId, string Id) : IRequest<FinancingDto>;

public class GetFinancingByIdHandler(IFinancingRepository financings) : IRequestHandler<GetFinancingByIdQuery, FinancingDto>
{
    public async Task<FinancingDto> Handle(GetFinancingByIdQuery query, CancellationToken ct) =>
        (await financings.GetByIdAsync(query.Id, query.UserId)
            ?? throw new NotFoundException("Financiamento nao encontrado")).ToDto();
}
