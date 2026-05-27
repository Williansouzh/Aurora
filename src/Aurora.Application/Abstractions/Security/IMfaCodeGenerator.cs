namespace Aurora.Application.Abstractions.Security;

public interface IMfaCodeGenerator
{
    string GenerateNumericCode(int digits = 6);
    string GenerateSecureToken(int bytes = 32);
    string HashSecret(string secret);
    bool VerifySecret(string secret, string hash);
}
