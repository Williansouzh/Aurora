namespace Aurora.Infrastructure.Security;

public class JwtSettings
{
    public string Issuer { get; set; } = "aurora";
    public string Audience { get; set; } = "aurora-client";
    public string Key { get; set; } = "CHANGE_ME_CHANGE_ME_CHANGE_ME_123";
    public int ExpiresMinutes { get; set; } = 120;
}
