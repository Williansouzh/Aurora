using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Messaging;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Auth.ResendMfa;

public record ResendMfaCommand(string ChallengeId) : IRequest<ResendMfaResponse>;

public record ResendMfaResponse(string ChallengeId);

public class ResendMfaCommandValidator : AbstractValidator<ResendMfaCommand>
{
    public ResendMfaCommandValidator() => RuleFor(x => x.ChallengeId).NotEmpty();
}

public class ResendMfaHandler(
    IAuthChallengeRepository challenges,
    IUserRepository users,
    IDateTimeProvider clock,
    IEncryptionService encryption,
    IMfaCodeGenerator codeGenerator,
    IEmailSender emailSender,
    IAuditService auditService) : IRequestHandler<ResendMfaCommand, ResendMfaResponse>
{
    public async Task<ResendMfaResponse> Handle(ResendMfaCommand command, CancellationToken ct)
    {
        var previous = await challenges.GetByIdAsync(command.ChallengeId, ct)
            ?? throw new UnauthorizedException("Desafio invalido");

        if (previous.Purpose != AuthChallengePurposes.EmailMfa || previous.IsConsumed)
        {
            throw new UnauthorizedException("Desafio invalido");
        }

        previous.ConsumedAt = clock.UtcNow;
        await challenges.UpdateAsync(previous, ct);

        var user = await users.GetByIdAsync(previous.UserId)
            ?? throw new UnauthorizedException("Usuario nao encontrado");
        var email = UserSecurityMapper.ReadEmail(user, encryption);
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
        await auditService.RecordAsync(user.Id, "mfa-challenge-resent", "AuthChallenge", challenge.Id, null, ct);
        return new ResendMfaResponse(challenge.Id);
    }
}
