namespace Aurora.Application.Features.Auth.Common;

using Aurora.Domain.Enums;

public record MeResponse(
    string UserId,
    string Name,
    string Email,
    bool IsEmailConfirmed = false,
    bool IsMfaEnabled = false,
    UserRole Role = UserRole.User,
    UserStatus Status = UserStatus.Active);
