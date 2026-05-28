using System.Security.Cryptography;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Domain.Entities;

namespace Aurora.Application.Features.Auth.Common;

public static class TokenHelper
{
    public static string HashToken(string raw, IEncryptionService encryption) =>
        encryption.HashDeterministic(raw);

    public static async Task<AuthResult> IssueTokens(
        User user,
        IJwtTokenService jwt,
        IRefreshTokenRepository refreshTokens,
        IDateTimeProvider clock,
        IEncryptionService encryption)
    {
        var accessToken = jwt.Generate(user);
        var rawRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        await refreshTokens.AddAsync(new RefreshToken
        {
            TokenHash = HashToken(rawRefresh, encryption),
            UserId = user.Id,
            ExpiresAt = clock.UtcNow.AddDays(7)
        });

        return new AuthResult(accessToken, jwt.ExpiresInSeconds, rawRefresh, user.Id, user.Name, user.Email);
    }
}
