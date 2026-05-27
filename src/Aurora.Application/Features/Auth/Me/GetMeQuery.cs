using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Auth.Me;

public record GetMeQuery(string UserId) : IRequest<MeResponse>;

public class GetMeHandler(IUserRepository users, IEncryptionService encryption)
    : IRequestHandler<GetMeQuery, MeResponse>
{
    public async Task<MeResponse> Handle(GetMeQuery query, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(query.UserId)
            ?? throw new NotFoundException("Usuário não encontrado");

        return new MeResponse(
            user.Id,
            user.Name,
            UserSecurityMapper.ReadEmail(user, encryption),
            user.IsEmailConfirmed,
            user.IsMfaEnabled);
    }
}
