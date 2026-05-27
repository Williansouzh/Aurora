namespace Aurora.Application.Features.Auth.Common;

public record AuthResult(
    string AccessToken,
    int ExpiresIn,
    string RawRefreshToken,
    string UserId,
    string Name,
    string Email,
    bool MfaRequired = false,
    string? ChallengeId = null);
