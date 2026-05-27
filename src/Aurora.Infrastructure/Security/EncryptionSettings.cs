namespace Aurora.Infrastructure.Security;

public class EncryptionSettings
{
    public string Key { get; set; } = "CHANGE_THIS_ENCRYPTION_KEY_32_BYTES_MIN";
    public string HashKey { get; set; } = "CHANGE_THIS_HASH_KEY_32_BYTES_MINIMUM";
}
