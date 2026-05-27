using Aurora.Domain.Exceptions;
using Aurora.Application.Interfaces;
using Aurora.Domain.Entities;
using MediatR;
using System.Security.Cryptography;
using System.Text;
namespace Aurora.Application.Features.Auth;
public record RegisterUserCommand(string Name,string Email,string Password):IRequest<AuthResult>;
public record LoginCommand(string Email,string Password):IRequest<AuthResult>;
public record GetMeQuery(string UserId):IRequest<MeResponse>;
public record RefreshTokenCommand(string RawToken):IRequest<AuthResult>;
public record LogoutCommand(string RawToken):IRequest;
public record UpdateProfileCommand(string UserId,string Name,string Email):IRequest<MeResponse>;
public record UpdatePasswordCommand(string UserId,string CurrentPassword,string NewPassword,string ConfirmPassword):IRequest;
public record AuthResult(string AccessToken,int ExpiresIn,string RawRefreshToken,string UserId,string Name,string Email);
public record MeResponse(string UserId,string Name,string Email);

public static class TokenHelper {
 public static string HashToken(string raw){var bytes=SHA256.HashData(Encoding.UTF8.GetBytes(raw)); return Convert.ToHexString(bytes).ToLowerInvariant();}
 public static async Task<AuthResult> IssueTokens(User u,IJwtTokenService jwt,IRefreshTokenRepository refreshTokens){
  var accessToken=jwt.Generate(u);
  var rawRefresh=Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
  var hash=HashToken(rawRefresh);
  await refreshTokens.AddAsync(new RefreshToken{TokenHash=hash,UserId=u.Id,ExpiresAt=DateTime.UtcNow.AddDays(7)});
  return new AuthResult(accessToken,jwt.ExpiresInSeconds,rawRefresh,u.Id,u.Name,u.Email);
 }
}

public class RegisterUserHandler(IUserRepository users,IPasswordHasher hasher,IJwtTokenService jwt,ICategoryRepository categories,IRefreshTokenRepository refreshTokens):IRequestHandler<RegisterUserCommand,AuthResult>{
 public async Task<AuthResult> Handle(RegisterUserCommand r,CancellationToken ct){
  if(await users.GetByEmailAsync(r.Email.ToLower()) is not null) throw new ConflictException("E-mail já cadastrado");
  var u=new User{Name=r.Name,Email=r.Email.ToLower(),PasswordHash=hasher.Hash(r.Password)};
  await users.AddAsync(u);
  await categories.SeedDefaultsAsync(u.Id);
  return await TokenHelper.IssueTokens(u,jwt,refreshTokens);
 }
}
public class LoginHandler(IUserRepository users,IPasswordHasher hasher,IJwtTokenService jwt,IRefreshTokenRepository refreshTokens):IRequestHandler<LoginCommand,AuthResult>{
 public async Task<AuthResult> Handle(LoginCommand r,CancellationToken ct){
  var u=await users.GetByEmailAsync(r.Email.ToLower())??throw new ValidationException("Credenciais inválidas");
  if(!hasher.Verify(r.Password,u.PasswordHash)) throw new ValidationException("Credenciais inválidas");
  return await TokenHelper.IssueTokens(u,jwt,refreshTokens);
 }
}
public class RefreshTokenHandler(IRefreshTokenRepository refreshTokens,IUserRepository users,IJwtTokenService jwt):IRequestHandler<RefreshTokenCommand,AuthResult>{
 public async Task<AuthResult> Handle(RefreshTokenCommand r,CancellationToken ct){
  if(string.IsNullOrWhiteSpace(r.RawToken)) throw new UnauthorizedException("Token inválido");
  var hash=TokenHelper.HashToken(r.RawToken);
  var token=await refreshTokens.GetByHashAsync(hash)??throw new UnauthorizedException("Token inválido");
  if(token.IsRevoked||token.ExpiresAt<=DateTime.UtcNow) throw new UnauthorizedException("Token expirado");
  var user=await users.GetByIdAsync(token.UserId)??throw new UnauthorizedException("Usuário não encontrado");
  await refreshTokens.RevokeAsync(token.Id);
  return await TokenHelper.IssueTokens(user,jwt,refreshTokens);
 }
}
public class LogoutHandler(IRefreshTokenRepository refreshTokens):IRequestHandler<LogoutCommand>{
 public async Task Handle(LogoutCommand r,CancellationToken ct){
  if(string.IsNullOrWhiteSpace(r.RawToken)) return;
  var hash=TokenHelper.HashToken(r.RawToken);
  var token=await refreshTokens.GetByHashAsync(hash);
  if(token is not null) await refreshTokens.RevokeAsync(token.Id);
 }
}
public class GetMeHandler(IUserRepository users):IRequestHandler<GetMeQuery,MeResponse>{public async Task<MeResponse> Handle(GetMeQuery r,CancellationToken ct){var u=await users.GetByIdAsync(r.UserId)??throw new NotFoundException("Usuário não encontrado"); return new MeResponse(u.Id,u.Name,u.Email);}}
public class UpdateProfileHandler(IUserRepository users):IRequestHandler<UpdateProfileCommand,MeResponse>{
 public async Task<MeResponse> Handle(UpdateProfileCommand c,CancellationToken ct){
  if(string.IsNullOrWhiteSpace(c.Name)) throw new ValidationException("Nome é obrigatório");
  if(string.IsNullOrWhiteSpace(c.Email)) throw new ValidationException("E-mail é obrigatório");
  var user=await users.GetByIdAsync(c.UserId)??throw new NotFoundException("Usuário não encontrado");
  var normalizedEmail=c.Email.Trim().ToLower();
  if(!string.Equals(user.Email,normalizedEmail,StringComparison.OrdinalIgnoreCase)){var existing=await users.GetByEmailAsync(normalizedEmail); if(existing is not null&&existing.Id!=user.Id) throw new ConflictException("E-mail já cadastrado");}
  user.Name=c.Name.Trim(); user.Email=normalizedEmail; user.UpdatedAt=DateTime.UtcNow;
  await users.UpdateAsync(user);
  return new MeResponse(user.Id,user.Name,user.Email);
 }
}
public class UpdatePasswordHandler(IUserRepository users,IPasswordHasher hasher):IRequestHandler<UpdatePasswordCommand>{
 public async Task Handle(UpdatePasswordCommand c,CancellationToken ct){
  if(string.IsNullOrWhiteSpace(c.CurrentPassword)) throw new ValidationException("Senha atual é obrigatória");
  if(string.IsNullOrWhiteSpace(c.NewPassword)||c.NewPassword.Length<8) throw new ValidationException("Nova senha deve ter ao menos 8 caracteres");
  if(c.NewPassword!=c.ConfirmPassword) throw new ValidationException("As senhas não conferem");
  var user=await users.GetByIdAsync(c.UserId)??throw new NotFoundException("Usuário não encontrado");
  if(!hasher.Verify(c.CurrentPassword,user.PasswordHash)) throw new ValidationException("Senha atual inválida");
  user.PasswordHash=hasher.Hash(c.NewPassword); user.UpdatedAt=DateTime.UtcNow;
  await users.UpdateAsync(user);
 }
}
