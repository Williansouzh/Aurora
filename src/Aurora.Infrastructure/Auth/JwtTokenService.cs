using Aurora.Application.Interfaces;
using Aurora.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace Aurora.Infrastructure.Auth;
public class JwtTokenService(IOptions<JwtSettings> s): IJwtTokenService {
 public int ExpiresInSeconds => s.Value.ExpiresMinutes * 60;
 public string Generate(User user){ var st=s.Value; var creds=new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(st.Key)),SecurityAlgorithms.HmacSha256);
 var claims = new[] { new Claim(ClaimTypes.NameIdentifier,user.Id), new Claim(ClaimTypes.Email,user.Email), new Claim(ClaimTypes.Name,user.Name)};
 var token = new JwtSecurityToken(st.Issuer, st.Audience, claims, expires:DateTime.UtcNow.AddMinutes(st.ExpiresMinutes), signingCredentials:creds);
 return new JwtSecurityTokenHandler().WriteToken(token);
 }
}
