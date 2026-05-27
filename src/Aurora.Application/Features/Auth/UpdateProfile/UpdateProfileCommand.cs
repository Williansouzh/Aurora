using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Auth.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Auth.UpdateProfile;

public record UpdateProfileCommand(string UserId, string Name, string Email) : IRequest<MeResponse>;

public class UpdateProfileHandler(IUserRepository users) : IRequestHandler<UpdateProfileCommand, MeResponse>
{
    public async Task<MeResponse> Handle(UpdateProfileCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Name)) throw new ValidationException("Nome é obrigatório");
        if (string.IsNullOrWhiteSpace(command.Email)) throw new ValidationException("E-mail é obrigatório");

        var user = await users.GetByIdAsync(command.UserId)
            ?? throw new NotFoundException("Usuário não encontrado");

        var normalizedEmail = command.Email.Trim().ToLower();

        if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await users.GetByEmailAsync(normalizedEmail);
            if (existing is not null && existing.Id != user.Id)
            {
                throw new ConflictException("E-mail já cadastrado");
            }
        }

        user.Name = command.Name.Trim();
        user.Email = normalizedEmail;
        user.UpdatedAt = DateTime.UtcNow;

        await users.UpdateAsync(user);
        return new MeResponse(user.Id, user.Name, user.Email);
    }
}
