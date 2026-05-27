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
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class RegisterUserHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtTokenService jwt,
    ICategoryRepository categories,
    IRefreshTokenRepository refreshTokens) : IRequestHandler<RegisterUserCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        if (await users.GetByEmailAsync(command.Email.ToLower()) is not null)
        {
            throw new ConflictException("E-mail já cadastrado");
        }

        var user = new User
        {
            Name = command.Name,
            Email = command.Email.ToLower(),
            PasswordHash = hasher.Hash(command.Password)
        };

        await users.AddAsync(user);
        await categories.SeedDefaultsAsync(user.Id);

        return await TokenHelper.IssueTokens(user, jwt, refreshTokens);
    }
}
