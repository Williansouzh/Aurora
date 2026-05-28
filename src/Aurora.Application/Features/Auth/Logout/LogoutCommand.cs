using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using MediatR;

namespace Aurora.Application.Features.Auth.Logout;

public record LogoutCommand(string RawToken) : IRequest;

public class LogoutHandler(
    IRefreshTokenRepository refreshTokens,
    IEncryptionService encryption,
    IAuditService auditService) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.RawToken))
        {
            return;
        }

        var token = await refreshTokens.GetByHashAsync(TokenHelper.HashToken(command.RawToken, encryption));
        if (token is null)
        {
            return;
        }

        await refreshTokens.RevokeAsync(token.Id);
        await auditService.RecordAsync(token.UserId, "logout", "RefreshToken", token.Id, null, ct);
    }
}
