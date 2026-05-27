using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Auth.Common;
using MediatR;

namespace Aurora.Application.Features.Auth.Logout;

public record LogoutCommand(string RawToken) : IRequest;

public class LogoutHandler(IRefreshTokenRepository refreshTokens) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.RawToken)) return;

        var hash = TokenHelper.HashToken(command.RawToken);
        var token = await refreshTokens.GetByHashAsync(hash);
        if (token is not null)
        {
            await refreshTokens.RevokeAsync(token.Id);
        }
    }
}
