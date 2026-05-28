using Aurora.Application.Abstractions.Common;
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
    IJwtTokenService jwt,
    IDateTimeProvider clock,
    IEncryptionService encryption,
    IAuditService auditService) : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.RawToken))
        {
            throw new UnauthorizedException("Token invalido");
        }

        var token = await refreshTokens.GetByHashAsync(TokenHelper.HashToken(command.RawToken, encryption))
            ?? throw new UnauthorizedException("Token invalido");

        if (token.IsRevoked)
        {
            await refreshTokens.RevokeAllByUserAsync(token.UserId);
            await auditService.RecordAsync(
                token.UserId,
                "refresh-token-reuse-detected",
                "RefreshToken",
                token.Id,
                null,
                ct);
            throw new UnauthorizedException("Token revogado");
        }

        if (token.ExpiresAt <= clock.UtcNow)
        {
            throw new UnauthorizedException("Token expirado");
        }

        var user = await users.GetByIdAsync(token.UserId)
            ?? throw new UnauthorizedException("Usuario nao encontrado");

        await refreshTokens.RevokeAsync(token.Id);
        await auditService.RecordAsync(user.Id, "refresh-token-rotated", "RefreshToken", token.Id, null, ct);

        return await TokenHelper.IssueTokens(user, jwt, refreshTokens, clock, encryption);
    }
}
