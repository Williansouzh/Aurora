using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Dashboard.CloseMonth;
using Aurora.Application.Features.Dashboard.GetHome;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, Route("api/home")]
public class HomeController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHome([FromQuery] int? month, [FromQuery] int? year)
    {
        var now = DateTime.UtcNow;
        return Ok(new ApiResponse<HomeDto>(true,
            await sender.Send(new GetHomeQuery(user.UserId, month ?? now.Month, year ?? now.Year))));
    }

    [HttpPost("close-month")]
    public async Task<IActionResult> CloseMonth([FromQuery] int? month, [FromQuery] int? year)
    {
        var now = DateTime.UtcNow;
        return Ok(new ApiResponse<CloseMonthResult>(true,
            await sender.Send(new CloseMonthCommand(user.UserId, month ?? now.Month, year ?? now.Year))));
    }
}
