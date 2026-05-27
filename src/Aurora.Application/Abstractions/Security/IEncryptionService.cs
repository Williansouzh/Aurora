namespace Aurora.Application.Abstractions.Security;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string HashDeterministic(string value);
}
