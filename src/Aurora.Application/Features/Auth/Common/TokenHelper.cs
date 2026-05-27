using System.Security.Cryptography;
using System.Text;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Domain.Entities;

namespace Aurora.Application.Features.Auth.Common;

public static class TokenHelper
{
    public static string HashToken(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static async Task<AuthResult> IssueTokens(
        User user,
        IJwtTokenService jwt,
        IRefreshTokenRepository refreshTokens)
    {
        var accessToken = jwt.Generate(user);
        var rawRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = HashToken(rawRefresh);

        await refreshTokens.AddAsync(new RefreshToken
        {
            TokenHash = hash,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        return new AuthResult(accessToken, jwt.ExpiresInSeconds, rawRefresh, user.Id, user.Name, user.Email);
    }
}
