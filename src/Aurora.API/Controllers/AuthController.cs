using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Auth.Common;
using Aurora.Application.Features.Auth.Login;
using Aurora.Application.Features.Auth.Logout;
using Aurora.Application.Features.Auth.Me;
using Aurora.Application.Features.Auth.Refresh;
using Aurora.Application.Features.Auth.Register;
using Aurora.Application.Features.Auth.UpdatePassword;
using Aurora.Application.Features.Auth.UpdateProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(
    ISender sender,
    IUserContext user,
    IWebHostEnvironment env,
    IRateLimiter rateLimiter) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand cmd)
    {
        var result = await sender.Send(cmd);
        SetRefreshCookie(result.RawRefreshToken);
        return Ok(new ApiResponse<AuthClientResponse>(true,
            new(result.AccessToken, result.ExpiresIn, result.UserId, result.Name, result.Email)));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand cmd)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (!await rateLimiter.IsAllowedAsync($"login:{ip}", 10, TimeSpan.FromMinutes(15)))
        {
            return StatusCode(429, new ApiResponse<string>(false, "Muitas tentativas. Tente novamente em 15 minutos."));
        }

        var result = await sender.Send(cmd);
        SetRefreshCookie(result.RawRefreshToken);
        return Ok(new ApiResponse<AuthClientResponse>(true,
            new(result.AccessToken, result.ExpiresIn, result.UserId, result.Name, result.Email)));
    }

    [AllowAnonymous, HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (!await rateLimiter.IsAllowedAsync($"refresh:{ip}", 10, TimeSpan.FromMinutes(15)))
        {
            return StatusCode(429, new ApiResponse<string>(false, "Muitas tentativas. Tente novamente em 15 minutos."));
        }

        var rawToken = Request.Cookies["aurora.refresh"];
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            ClearRefreshCookie();
            return Unauthorized(new ApiResponse<string>(false, "Token não encontrado."));
        }

        try
        {
            var result = await sender.Send(new RefreshTokenCommand(rawToken));
            SetRefreshCookie(result.RawRefreshToken);
            return Ok(new ApiResponse<AuthClientResponse>(true,
                new(result.AccessToken, result.ExpiresIn, result.UserId, result.Name, result.Email)));
        }
        catch
        {
            ClearRefreshCookie();
            return Unauthorized(new ApiResponse<string>(false, "Token inválido ou expirado."));
        }
    }

    [AllowAnonymous, HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var rawToken = Request.Cookies["aurora.refresh"];
        if (!string.IsNullOrWhiteSpace(rawToken))
        {
            await sender.Send(new LogoutCommand(rawToken));
        }
        ClearRefreshCookie();
        return NoContent();
    }

    [Authorize, HttpGet("me")]
    public async Task<IActionResult> Me() =>
        Ok(new ApiResponse<MeResponse>(true, await sender.Send(new GetMeQuery(user.UserId))));

    [Authorize, HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest req) =>
        Ok(new ApiResponse<MeResponse>(true,
            await sender.Send(new UpdateProfileCommand(user.UserId, req.Name, req.Email))));

    [Authorize, HttpPut("password")]
    public async Task<IActionResult> UpdatePassword(UpdatePasswordRequest req)
    {
        await sender.Send(new UpdatePasswordCommand(user.UserId, req.CurrentPassword, req.NewPassword, req.ConfirmPassword));
        return Ok(new ApiResponse<string>(true, "password-updated"));
    }

    private void SetRefreshCookie(string rawToken)
    {
        var isDev = env.IsDevelopment();
        Response.Cookies.Append("aurora.refresh", rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDev,
            SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.Strict,
            Path = "/api/auth",
            MaxAge = TimeSpan.FromDays(7),
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    private void ClearRefreshCookie()
    {
        var isDev = env.IsDevelopment();
        Response.Cookies.Append("aurora.refresh", string.Empty, new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDev,
            SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.Strict,
            Path = "/api/auth",
            MaxAge = TimeSpan.Zero,
            Expires = DateTimeOffset.UnixEpoch
        });
    }
}

public record AuthClientResponse(string AccessToken, int ExpiresIn, string UserId, string Name, string Email);
public record UpdateProfileRequest(string Name, string Email);
public record UpdatePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
