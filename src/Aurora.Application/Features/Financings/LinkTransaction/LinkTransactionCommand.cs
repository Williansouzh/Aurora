using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Financings.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Financings.LinkTransaction;

public record LinkTransactionCommand(string UserId, string FinancingId, int Number, string TransactionId)
    : IRequest<FinancingDto>;

public class LinkTransactionHandler(IFinancingRepository financings) : IRequestHandler<LinkTransactionCommand, FinancingDto>
{
    public async Task<FinancingDto> Handle(LinkTransactionCommand command, CancellationToken ct)
    {
        var f = await financings.GetByIdAsync(command.FinancingId, command.UserId)
            ?? throw new NotFoundException("Financiamento nao encontrado");

        var inst = f.Installments.FirstOrDefault(x => x.Number == command.Number)
            ?? throw new NotFoundException("Parcela nao encontrada");

        inst.LinkedTransactionId = command.TransactionId;

        await financings.UpdateAsync(f);
        return f.ToDto();
    }
}
