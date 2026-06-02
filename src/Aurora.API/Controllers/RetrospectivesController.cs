using Aurora.API.Authorization;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Retrospectives.GetMonthly;
using Aurora.Application.Features.Retrospectives.GetWeekly;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, RequireModule(ModuleKeys.Retrospectives), Route("api/retrospectives")]
public class RetrospectivesController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet("weekly")]
    public async Task<IActionResult> Weekly([FromQuery] DateTime? weekStart)
    {
        var start = weekStart?.Date ?? StartOfCurrentWeek();
        return Ok(new ApiResponse<WeeklyRetrospectiveDto>(true,
            await sender.Send(new GetWeeklyRetrospectiveQuery(user.UserId, start))));
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> Monthly([FromQuery] int? month, [FromQuery] int? year)
    {
        var now = DateTime.UtcNow;
        return Ok(new ApiResponse<MonthlyRetrospectiveDto>(true,
            await sender.Send(new GetMonthlyRetrospectiveQuery(user.UserId, month ?? now.Month, year ?? now.Year))));
    }

    private static DateTime StartOfCurrentWeek()
    {
        var today = DateTime.UtcNow.Date;
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        return today.AddDays(-diff);
    }
}
