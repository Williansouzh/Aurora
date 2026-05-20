using Aurora.Application.Common;
using Aurora.Application.Features.Auth;
using Aurora.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;
[ApiController, Route("api/auth")]
public class AuthController(ISender sender, IUserContext user): ControllerBase {
 [HttpPost("register")] public async Task<IActionResult> Register(RegisterUserCommand cmd)=>Ok(new ApiResponse<AuthResponse>(true, await sender.Send(cmd)));
 [HttpPost("login")] public async Task<IActionResult> Login(LoginCommand cmd)=>Ok(new ApiResponse<AuthResponse>(true, await sender.Send(cmd)));
 [Authorize, HttpGet("me")] public async Task<IActionResult> Me()=>Ok(new ApiResponse<MeResponse>(true, await sender.Send(new GetMeQuery(user.UserId))));
}
