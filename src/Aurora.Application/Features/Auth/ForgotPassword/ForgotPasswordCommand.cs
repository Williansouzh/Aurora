using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Messaging;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Auth.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator() => RuleFor(x => x.Email).NotEmpty().EmailAddress();
}

public class ForgotPasswordHandler(
    IUserRepository users,
    IAuthChallengeRepository challenges,
    IEmailSender emailSender,
    IDateTimeProvider clock,
    IEncryptionService encryption,
    IMfaCodeGenerator codeGenerator) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand command, CancellationToken ct)
    {
        var email = UserSecurityMapper.NormalizeEmail(command.Email);
        var user = await users.GetByEmailHashAsync(encryption.HashDeterministic(email)) ??
            await users.GetByEmailAsync(email);

        if (user is null)
        {
            return;
        }

        var token = codeGenerator.GenerateSecureToken();
        await challenges.AddAsync(new AuthChallenge
        {
            UserId = user.Id,
            Purpose = AuthChallengePurposes.PasswordReset,
            CodeHash = codeGenerator.HashSecret(token),
            TokenHash = codeGenerator.HashSecret(token),
            ExpiresAt = clock.UtcNow.AddMinutes(30),
            MaxAttempts = 5
        }, ct);

        await emailSender.SendAsync(
            email,
            "Redefinicao de senha Aurora",
            $"Use este token para redefinir sua senha: {token}",
            ct);
    }
}
