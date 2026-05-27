using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Accounts.Common;
using MediatR;

namespace Aurora.Application.Features.Accounts.GetAll;

public record GetAccountsQuery(string UserId) : IRequest<List<AccountDto>>;

public class GetAccountsHandler(IAccountRepository accounts) : IRequestHandler<GetAccountsQuery, List<AccountDto>>
{
    public async Task<List<AccountDto>> Handle(GetAccountsQuery query, CancellationToken ct) =>
        (await accounts.GetByUserAsync(query.UserId)).Select(x => x.ToDto()).ToList();
}
