using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Accounts.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Accounts.GetById;

public record GetAccountByIdQuery(string UserId, string Id) : IRequest<AccountDto>;

public class GetAccountByIdHandler(IAccountRepository accounts) : IRequestHandler<GetAccountByIdQuery, AccountDto>
{
    public async Task<AccountDto> Handle(GetAccountByIdQuery query, CancellationToken ct) =>
        (await accounts.GetByIdAsync(query.Id, query.UserId)
            ?? throw new NotFoundException("Conta nao encontrada")).ToDto();
}
