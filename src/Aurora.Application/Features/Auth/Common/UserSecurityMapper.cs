using Aurora.Application.Abstractions.Security;
using Aurora.Domain.Entities;
using Aurora.Domain.ValueObjects;

namespace Aurora.Application.Features.Auth.Common;

public static class UserSecurityMapper
{
    public static string NormalizeEmail(string email) => Email.Create(email).Value;

    public static void SetEmail(User user, string email, IEncryptionService encryption)
    {
        var normalized = NormalizeEmail(email);
        user.Email = normalized;
        user.EmailHash = encryption.HashDeterministic(normalized);
        user.EmailEncrypted = encryption.Encrypt(normalized);
    }

    public static string ReadEmail(User user, IEncryptionService encryption)
    {
        if (!string.IsNullOrWhiteSpace(user.EmailEncrypted))
        {
            return encryption.Decrypt(user.EmailEncrypted);
        }

        return user.Email;
    }
}
