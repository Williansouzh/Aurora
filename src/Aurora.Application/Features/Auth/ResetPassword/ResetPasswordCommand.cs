using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Auth.ResetPassword;

public record ResetPasswordCommand(string Token, string NewPassword, string ConfirmPassword) : IRequest;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(10);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword);
    }
}

public class ResetPasswordHandler(
    IAuthChallengeRepository challenges,
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IMfaCodeGenerator codeGenerator,
    IDateTimeProvider clock,
    IRefreshTokenRepository refreshTokens,
    IAuditService auditService) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        var tokenHash = codeGenerator.HashSecret(command.Token);
        var challenge = await challenges.GetByTokenHashAsync(
            tokenHash,
            AuthChallengePurposes.PasswordReset,
            ct) ?? throw new UnauthorizedException("Token invalido");

        if (challenge.IsConsumed || challenge.IsExpired(clock.UtcNow))
        {
            throw new UnauthorizedException("Token invalido ou expirado");
        }

        var user = await users.GetByIdAsync(challenge.UserId)
            ?? throw new UnauthorizedException("Usuario nao encontrado");

        user.PasswordHash = passwordHasher.Hash(command.NewPassword);
        user.UpdatedAt = clock.UtcNow;
        challenge.ConsumedAt = clock.UtcNow;

        await users.UpdateAsync(user);
        await challenges.UpdateAsync(challenge, ct);
        await refreshTokens.RevokeAllByUserAsync(user.Id);
        await auditService.RecordAsync(user.Id, "password-reset", "User", user.Id, null, ct);
    }
}
