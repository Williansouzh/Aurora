using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Habits.CheckIn;
using Aurora.Application.Features.Habits.Common;
using Aurora.Application.Features.Habits.Create;
using Aurora.Application.Features.Habits.Delete;
using Aurora.Application.Features.Habits.GetAll;
using Aurora.Application.Features.Habits.GetStats;
using Aurora.Application.Features.Habits.Pause;
using Aurora.Application.Features.Habits.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, Route("api/habits")]
public class HabitsController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true) =>
        Ok(new ApiResponse<List<HabitDto>>(true, await sender.Send(new GetHabitsQuery(user.UserId, activeOnly))));

    [HttpPost]
    public async Task<IActionResult> Create(CreateHabitCommand req) =>
        Ok(new ApiResponse<HabitDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateHabitCommand req) =>
        Ok(new ApiResponse<HabitDto>(true, await sender.Send(req with { UserId = user.UserId, Id = id })));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await sender.Send(new DeleteHabitCommand(user.UserId, id));
        return Ok(new ApiResponse<string>(true, "deleted"));
    }

    [HttpPatch("{id}/pause")]
    public async Task<IActionResult> Pause(string id) =>
        Ok(new ApiResponse<HabitDto>(true, await sender.Send(new PauseHabitCommand(user.UserId, id, true))));

    [HttpPatch("{id}/resume")]
    public async Task<IActionResult> Resume(string id) =>
        Ok(new ApiResponse<HabitDto>(true, await sender.Send(new PauseHabitCommand(user.UserId, id, false))));

    [HttpPost("{id}/check-in")]
    public async Task<IActionResult> CheckIn(string id, HabitCheckInCommand req) =>
        Ok(new ApiResponse<HabitCheckInDto>(true, await sender.Send(req with { UserId = user.UserId, HabitId = id })));

    [HttpGet("{id}/stats")]
    public async Task<IActionResult> Stats(string id, [FromQuery] int? year, [FromQuery] int? month)
    {
        var now = DateTime.UtcNow;
        return Ok(new ApiResponse<HabitStatsDto>(true,
            await sender.Send(new GetHabitStatsQuery(user.UserId, id, year ?? now.Year, month ?? now.Month))));
    }
}
