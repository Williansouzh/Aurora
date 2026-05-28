using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Messaging;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Domain.Entities;
using FluentValidation;
using MediatR;
using ValidationException = Aurora.Domain.Exceptions.ValidationException;

namespace Aurora.Application.Features.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtTokenService jwt,
    IRefreshTokenRepository refreshTokens,
    IDateTimeProvider clock,
    IEncryptionService encryption,
    IAuthChallengeRepository challenges,
    IEmailSender emailSender,
    IMfaCodeGenerator codeGenerator,
    IRateLimiter rateLimiter,
    IAuditService auditService) : IRequestHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(LoginCommand command, CancellationToken ct)
    {
        var normalizedEmail = UserSecurityMapper.NormalizeEmail(command.Email);
        var userKey = encryption.HashDeterministic(normalizedEmail);

        if (!await rateLimiter.IsAllowedAsync($"lockout:user:{userKey}", 5, TimeSpan.FromMinutes(15)))
        {
            throw new ValidationException("Muitas tentativas. Tente novamente em 15 minutos.");
        }

        var user = await users.GetByEmailHashAsync(userKey) ??
            await users.GetByEmailAsync(normalizedEmail) ??
            throw new ValidationException("Credenciais invalidas");

        if (!hasher.Verify(command.Password, user.PasswordHash))
        {
            await auditService.RecordAsync(user.Id, "login-failed", "User", user.Id, null, ct);
            throw new ValidationException("Credenciais invalidas");
        }

        if (!string.IsNullOrWhiteSpace(user.EmailEncrypted) && string.IsNullOrWhiteSpace(user.EmailHash))
        {
            UserSecurityMapper.SetEmail(user, normalizedEmail, encryption);
            await users.UpdateAsync(user);
        }

        if (user.IsMfaEnabled)
        {
            var challenge = await CreateMfaChallengeAsync(user, normalizedEmail, ct);
            await auditService.RecordAsync(user.Id, "mfa-challenge-created", "AuthChallenge", challenge.Id, null, ct);
            return new AuthResult(
                string.Empty,
                0,
                string.Empty,
                user.Id,
                user.Name,
                normalizedEmail,
                true,
                challenge.Id);
        }

        await auditService.RecordAsync(user.Id, "login-success", "User", user.Id, null, ct);
        return await TokenHelper.IssueTokens(user, jwt, refreshTokens, clock, encryption);
    }

    private async Task<AuthChallenge> CreateMfaChallengeAsync(User user, string email, CancellationToken ct)
    {
        var code = codeGenerator.GenerateNumericCode();
        var challenge = new AuthChallenge
        {
            UserId = user.Id,
            Purpose = AuthChallengePurposes.EmailMfa,
            CodeHash = codeGenerator.HashSecret(code),
            ExpiresAt = clock.UtcNow.AddMinutes(5),
            MaxAttempts = 5
        };

        await challenges.AddAsync(challenge, ct);
        await emailSender.SendAsync(email, "Seu codigo Aurora", $"Codigo de acesso: {code}", ct);
        return challenge;
    }
}
