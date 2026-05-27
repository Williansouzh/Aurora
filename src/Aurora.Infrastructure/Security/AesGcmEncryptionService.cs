using System.Security.Cryptography;
using System.Text;
using Aurora.Application.Abstractions.Security;
using Microsoft.Extensions.Options;

namespace Aurora.Infrastructure.Security;

public class AesGcmEncryptionService(IOptions<EncryptionSettings> settings) : IEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        var key = DeriveKey(settings.Value.Key);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        return Convert.ToBase64String(nonce.Concat(tag).Concat(cipherBytes).ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return string.Empty;
        }

        var payload = Convert.FromBase64String(cipherText);
        var nonce = payload[..NonceSize];
        var tag = payload[NonceSize..(NonceSize + TagSize)];
        var cipherBytes = payload[(NonceSize + TagSize)..];
        var plainBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(DeriveKey(settings.Value.Key), TagSize);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public string HashDeterministic(string value)
    {
        var key = DeriveKey(settings.Value.HashKey);
        using var hmac = new HMACSHA256(key);
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(value.Trim().ToLowerInvariant())))
            .ToLowerInvariant();
    }

    private static byte[] DeriveKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Encryption key is required.");
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(key));
    }
}
