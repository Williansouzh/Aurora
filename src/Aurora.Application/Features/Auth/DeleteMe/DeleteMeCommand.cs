using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Auth.DeleteMe;

public record DeleteMeCommand(string UserId, string? Reason = null) : IRequest;

public class DeleteMeHandler(
    IUserRepository users,
    IDateTimeProvider clock,
    IAuditService auditService,
    IRefreshTokenRepository refreshTokens) : IRequestHandler<DeleteMeCommand>
{
    public async Task Handle(DeleteMeCommand command, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(command.UserId)
            ?? throw new NotFoundException("Usuario nao encontrado");

        user.Name = "Deleted user";
        user.Email = $"deleted-{user.Id}@anonymous.local";
        user.EmailHash = string.Empty;
        user.EmailEncrypted = string.Empty;
        user.PasswordHash = string.Empty;
        user.IsEmailConfirmed = false;
        user.IsMfaEnabled = false;
        user.DeletedAt = clock.UtcNow;
        user.DeletionReason = command.Reason;

        await users.UpdateAsync(user);
        await refreshTokens.RevokeAllByUserAsync(user.Id);
        await auditService.RecordAsync(user.Id, "delete-me", "User", user.Id, command.Reason, ct);
    }
}
