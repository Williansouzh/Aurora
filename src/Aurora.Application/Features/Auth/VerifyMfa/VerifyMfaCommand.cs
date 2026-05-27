using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Auth.VerifyMfa;

public record VerifyMfaCommand(string ChallengeId, string Code) : IRequest<AuthResult>;

public class VerifyMfaCommandValidator : AbstractValidator<VerifyMfaCommand>
{
    public VerifyMfaCommandValidator()
    {
        RuleFor(x => x.ChallengeId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(6);
    }
}

public class VerifyMfaHandler(
    IAuthChallengeRepository challenges,
    IUserRepository users,
    IJwtTokenService jwt,
    IRefreshTokenRepository refreshTokens,
    IDateTimeProvider clock,
    IMfaCodeGenerator codeGenerator,
    IEncryptionService encryption) : IRequestHandler<VerifyMfaCommand, AuthResult>
{
    public async Task<AuthResult> Handle(VerifyMfaCommand command, CancellationToken ct)
    {
        var challenge = await challenges.GetByIdAsync(command.ChallengeId, ct)
            ?? throw new UnauthorizedException("Desafio invalido");

        if (challenge.Purpose != AuthChallengePurposes.EmailMfa ||
            challenge.IsConsumed ||
            challenge.IsExpired(clock.UtcNow) ||
            challenge.Attempts >= challenge.MaxAttempts)
        {
            throw new UnauthorizedException("Desafio invalido ou expirado");
        }

        challenge.Attempts++;
        if (!codeGenerator.VerifySecret(command.Code, challenge.CodeHash))
        {
            await challenges.UpdateAsync(challenge, ct);
            throw new UnauthorizedException("Codigo invalido");
        }

        challenge.ConsumedAt = clock.UtcNow;
        await challenges.UpdateAsync(challenge, ct);

        var user = await users.GetByIdAsync(challenge.UserId)
            ?? throw new UnauthorizedException("Usuario nao encontrado");

        var result = await TokenHelper.IssueTokens(user, jwt, refreshTokens, clock);
        return result with { Email = UserSecurityMapper.ReadEmail(user, encryption) };
    }
}
