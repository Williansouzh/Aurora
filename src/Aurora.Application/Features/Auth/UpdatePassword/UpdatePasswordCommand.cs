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

public class UpdatePasswordHandler(IUserRepository users, IPasswordHasher hasher) : IRequestHandler<UpdatePasswordCommand>
{
    public async Task Handle(UpdatePasswordCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.CurrentPassword))
        {
            throw new ValidationException("Senha atual é obrigatória");
        }

        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < 8)
        {
            throw new ValidationException("Nova senha deve ter ao menos 8 caracteres");
        }

        if (command.NewPassword != command.ConfirmPassword)
        {
            throw new ValidationException("As senhas não conferem");
        }

        var user = await users.GetByIdAsync(command.UserId)
            ?? throw new NotFoundException("Usuário não encontrado");

        if (!hasher.Verify(command.CurrentPassword, user.PasswordHash))
        {
            throw new ValidationException("Senha atual inválida");
        }

        user.PasswordHash = hasher.Hash(command.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await users.UpdateAsync(user);
    }
}
