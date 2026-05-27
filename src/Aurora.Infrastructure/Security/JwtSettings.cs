namespace Aurora.Infrastructure.Security;

public class JwtSettings
{
    public string Issuer { get; set; } = "aurora";
    public string Audience { get; set; } = "aurora-client";
    public string Key { get; set; } = "CHANGE_ME_CHANGE_ME_CHANGE_ME_123";
    public string CurrentKeyId { get; set; } = "default";
    public List<JwtSigningKey> Keys { get; set; } = [];
    public int ExpiresMinutes { get; set; } = 120;
}

public class JwtSigningKey
{
    public string KeyId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}
