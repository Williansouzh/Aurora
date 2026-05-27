using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Transactions.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Transactions.GetById;

public record GetTransactionByIdQuery(string UserId, string Id) : IRequest<TransactionDto>;

public class GetTransactionByIdHandler(ITransactionRepository transactions) : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    public async Task<TransactionDto> Handle(GetTransactionByIdQuery query, CancellationToken ct) =>
        (await transactions.GetByIdAsync(query.Id, query.UserId)
            ?? throw new NotFoundException("Transacao nao encontrada")).ToDto();
}
