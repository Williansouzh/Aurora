using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Aurora.Application.Abstractions.Security;
using Aurora.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Aurora.Infrastructure.Security;

public class JwtTokenService(IOptions<JwtSettings> settings) : IJwtTokenService
{
    public int ExpiresInSeconds => settings.Value.ExpiresMinutes * 60;

    public string Generate(User user)
    {
        var st = settings.Value;
        var activeKey = st.Keys.FirstOrDefault(x => x.KeyId == st.CurrentKeyId) ??
            new JwtSigningKey { KeyId = st.CurrentKeyId, Key = st.Key };
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(activeKey.Key)) { KeyId = activeKey.KeyId },
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            st.Issuer,
            st.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(st.ExpiresMinutes),
            signingCredentials: creds);

        token.Header["kid"] = activeKey.KeyId;

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
