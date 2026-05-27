using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Auth.ConfirmEmail;

public record ConfirmEmailCommand(string Token) : IRequest;

public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator() => RuleFor(x => x.Token).NotEmpty();
}

public class ConfirmEmailHandler(
    IAuthChallengeRepository challenges,
    IUserRepository users,
    IMfaCodeGenerator codeGenerator,
    IDateTimeProvider clock) : IRequestHandler<ConfirmEmailCommand>
{
    public async Task Handle(ConfirmEmailCommand command, CancellationToken ct)
    {
        var challenge = await challenges.GetByTokenHashAsync(
            codeGenerator.HashSecret(command.Token),
            AuthChallengePurposes.EmailConfirmation,
            ct) ?? throw new UnauthorizedException("Token invalido");

        if (challenge.IsConsumed || challenge.IsExpired(clock.UtcNow))
        {
            throw new UnauthorizedException("Token invalido ou expirado");
        }

        var user = await users.GetByIdAsync(challenge.UserId)
            ?? throw new UnauthorizedException("Usuario nao encontrado");

        user.IsEmailConfirmed = true;
        user.EmailConfirmedAt = clock.UtcNow;
        challenge.ConsumedAt = clock.UtcNow;

        await users.UpdateAsync(user);
        await challenges.UpdateAsync(challenge, ct);
    }
}
