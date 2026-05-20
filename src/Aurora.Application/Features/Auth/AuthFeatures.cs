using Aurora.Domain.Exceptions;
using Aurora.Application.Interfaces;
using Aurora.Domain.Entities;
using MediatR;
namespace Aurora.Application.Features.Auth;
public record RegisterUserCommand(string Name,string Email,string Password):IRequest<AuthResponse>;
public record LoginCommand(string Email,string Password):IRequest<AuthResponse>;
public record GetMeQuery(string UserId):IRequest<MeResponse>;
public record AuthResponse(string Token,string UserId,string Name,string Email);
public record MeResponse(string UserId,string Name,string Email);

public class RegisterUserHandler(IUserRepository users, IPasswordHasher hasher, IJwtTokenService jwt, ICategoryRepository categories): IRequestHandler<RegisterUserCommand, AuthResponse>{
 public async Task<AuthResponse> Handle(RegisterUserCommand r, CancellationToken ct){ if(await users.GetByEmailAsync(r.Email) is not null) throw new ConflictException("E-mail já cadastrado"); var u=new User{Name=r.Name,Email=r.Email.ToLower(),PasswordHash=hasher.Hash(r.Password)}; await users.AddAsync(u); await categories.SeedDefaultsAsync(u.Id); return new AuthResponse(jwt.Generate(u),u.Id,u.Name,u.Email);} }
public class LoginHandler(IUserRepository users, IPasswordHasher hasher, IJwtTokenService jwt): IRequestHandler<LoginCommand, AuthResponse>{ public async Task<AuthResponse> Handle(LoginCommand r, CancellationToken ct){ var u=await users.GetByEmailAsync(r.Email.ToLower())??throw new ValidationException("Credenciais inválidas"); if(!hasher.Verify(r.Password,u.PasswordHash)) throw new ValidationException("Credenciais inválidas"); return new AuthResponse(jwt.Generate(u),u.Id,u.Name,u.Email);} }
public class GetMeHandler(IUserRepository users): IRequestHandler<GetMeQuery, MeResponse>{ public async Task<MeResponse> Handle(GetMeQuery r, CancellationToken ct){ var u=await users.GetByIdAsync(r.UserId)??throw new NotFoundException("Usuário não encontrado"); return new MeResponse(u.Id,u.Name,u.Email);} }
