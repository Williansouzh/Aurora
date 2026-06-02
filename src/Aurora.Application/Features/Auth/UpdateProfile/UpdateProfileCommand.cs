using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Auth.UpdateProfile;

public record UpdateProfileCommand(string UserId, string Name, string Email) : IRequest<MeResponse>;

public class UpdateProfileHandler(
    IUserRepository users,
    IEncryptionService encryption,
    IDateTimeProvider clock,
    IAuditService auditService) : IRequestHandler<UpdateProfileCommand, MeResponse>
{
    public async Task<MeResponse> Handle(UpdateProfileCommand command, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(command.UserId)
            ?? throw new NotFoundException("Usuario nao encontrado");

        var normalizedEmail = UserSecurityMapper.NormalizeEmail(command.Email);
        var emailHash = encryption.HashDeterministic(normalizedEmail);

        if (!string.Equals(user.EmailHash, emailHash, StringComparison.Ordinal))
        {
            var existing = await users.GetByEmailHashAsync(emailHash) ??
                await users.GetByEmailAsync(normalizedEmail);
            if (existing is not null && existing.Id != user.Id)
            {
                throw new ConflictException("E-mail ja cadastrado");
            }

            user.IsEmailConfirmed = false;
            user.EmailConfirmedAt = null;
        }

        user.Name = command.Name.Trim();
        UserSecurityMapper.SetEmail(user, normalizedEmail, encryption);
        user.UpdatedAt = clock.UtcNow;

        await users.UpdateAsync(user);
        await auditService.RecordAsync(user.Id, "profile-updated", "User", user.Id, null, ct);

        return new MeResponse(user.Id, user.Name, normalizedEmail, user.IsEmailConfirmed, user.IsMfaEnabled, user.Role, user.Status);
    }
}
