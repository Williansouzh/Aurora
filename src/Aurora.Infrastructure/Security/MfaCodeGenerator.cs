using System.Security.Cryptography;
using System.Text;
using Aurora.Application.Abstractions.Security;

namespace Aurora.Infrastructure.Security;

public class MfaCodeGenerator : IMfaCodeGenerator
{
    public string GenerateNumericCode(int digits = 6)
    {
        var max = (int)Math.Pow(10, digits);
        var value = RandomNumberGenerator.GetInt32(0, max);
        return value.ToString($"D{digits}");
    }

    public string GenerateSecureToken(int bytes = 32) =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(bytes));

    public string HashSecret(string secret)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool VerifySecret(string secret, string hash) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(HashSecret(secret)),
            Encoding.UTF8.GetBytes(hash));
}
