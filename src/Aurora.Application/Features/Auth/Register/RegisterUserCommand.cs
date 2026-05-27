using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Messaging;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Auth.Register;

public record RegisterUserCommand(string Name, string Email, string Password) : IRequest<AuthResult>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(10);
    }
}

public class RegisterUserHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtTokenService jwt,
    ICategoryRepository categories,
    IRefreshTokenRepository refreshTokens,
    IDateTimeProvider clock,
    IEncryptionService encryption,
    IAuthChallengeRepository challenges,
    IEmailSender emailSender,
    IMfaCodeGenerator codeGenerator) : IRequestHandler<RegisterUserCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        var normalizedEmail = UserSecurityMapper.NormalizeEmail(command.Email);
        var emailHash = encryption.HashDeterministic(normalizedEmail);

        if (await users.GetByEmailHashAsync(emailHash) is not null ||
            await users.GetByEmailAsync(normalizedEmail) is not null)
        {
            throw new ConflictException("E-mail ja cadastrado");
        }

        var user = new User
        {
            Name = command.Name.Trim(),
            PasswordHash = hasher.Hash(command.Password)
        };
        UserSecurityMapper.SetEmail(user, normalizedEmail, encryption);

        await users.AddAsync(user);
        await categories.SeedDefaultsAsync(user.Id);
        await SendEmailConfirmationAsync(user, normalizedEmail, ct);

        return await TokenHelper.IssueTokens(user, jwt, refreshTokens, clock);
    }

    private async Task SendEmailConfirmationAsync(User user, string email, CancellationToken ct)
    {
        var token = codeGenerator.GenerateSecureToken();
        var challenge = new AuthChallenge
        {
            UserId = user.Id,
            Purpose = AuthChallengePurposes.EmailConfirmation,
            CodeHash = codeGenerator.HashSecret(token),
            TokenHash = codeGenerator.HashSecret(token),
            ExpiresAt = clock.UtcNow.AddHours(24),
            MaxAttempts = 10
        };

        await challenges.AddAsync(challenge, ct);
        await emailSender.SendAsync(
            email,
            "Confirme seu e-mail no Aurora",
            $"Use este token para confirmar seu e-mail: {token}",
            ct);
    }
}
