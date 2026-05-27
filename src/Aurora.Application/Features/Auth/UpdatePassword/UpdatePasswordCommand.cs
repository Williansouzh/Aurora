using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Auth.UpdatePassword;

public record UpdatePasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword) : IRequest;

public class UpdatePasswordHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    IDateTimeProvider clock,
    IAuditService auditService,
    IRefreshTokenRepository refreshTokens) : IRequestHandler<UpdatePasswordCommand>
{
    public async Task Handle(UpdatePasswordCommand command, CancellationToken ct)
    {
        if (command.NewPassword != command.ConfirmPassword)
        {
            throw new ValidationException("As senhas nao conferem");
        }

        var user = await users.GetByIdAsync(command.UserId)
            ?? throw new NotFoundException("Usuario nao encontrado");

        if (!hasher.Verify(command.CurrentPassword, user.PasswordHash))
        {
            throw new ValidationException("Senha atual invalida");
        }

        user.PasswordHash = hasher.Hash(command.NewPassword);
        user.UpdatedAt = clock.UtcNow;
        await users.UpdateAsync(user);
        await refreshTokens.RevokeAllByUserAsync(user.Id);
        await auditService.RecordAsync(user.Id, "password-updated", "User", user.Id, null, ct);
    }
}
