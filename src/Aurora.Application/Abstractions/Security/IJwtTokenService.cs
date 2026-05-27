using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Security;

public interface IJwtTokenService
{
    string Generate(User user);
    int ExpiresInSeconds { get; }
}
