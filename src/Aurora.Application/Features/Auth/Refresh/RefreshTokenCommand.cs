using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Auth.Refresh;

public record RefreshTokenCommand(string RawToken) : IRequest<AuthResult>;

public class RefreshTokenHandler(
    IRefreshTokenRepository refreshTokens,
    IUserRepository users,
    IJwtTokenService jwt) : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.RawToken))
        {
            throw new UnauthorizedException("Token inválido");
        }

        var hash = TokenHelper.HashToken(command.RawToken);
        var token = await refreshTokens.GetByHashAsync(hash)
            ?? throw new UnauthorizedException("Token inválido");

        if (token.IsRevoked || token.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedException("Token expirado");
        }

        var user = await users.GetByIdAsync(token.UserId)
            ?? throw new UnauthorizedException("Usuário não encontrado");

        await refreshTokens.RevokeAsync(token.Id);

        return await TokenHelper.IssueTokens(user, jwt, refreshTokens);
    }
}
