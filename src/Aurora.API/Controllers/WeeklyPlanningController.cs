using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.WeeklyPlanning.Close;
using Aurora.Application.Features.WeeklyPlanning.Common;
using Aurora.Application.Features.WeeklyPlanning.Create;
using Aurora.Application.Features.WeeklyPlanning.GetAll;
using Aurora.Application.Features.WeeklyPlanning.GetCurrent;
using Aurora.Application.Features.WeeklyPlanning.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, Route("api/weekly-planning")]
public class WeeklyPlanningController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent() =>
        Ok(new ApiResponse<WeeklyPlanDto?>(true, await sender.Send(new GetCurrentWeeklyPlanQuery(user.UserId))));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int limit = 10) =>
        Ok(new ApiResponse<List<WeeklyPlanDto>>(true, await sender.Send(new GetWeeklyPlansQuery(user.UserId, limit))));

    [HttpPost]
    public async Task<IActionResult> Create(CreateWeeklyPlanCommand req) =>
        Ok(new ApiResponse<WeeklyPlanDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateWeeklyPlanCommand req) =>
        Ok(new ApiResponse<WeeklyPlanDto>(true, await sender.Send(req with { UserId = user.UserId, Id = id })));

    [HttpPatch("{id}/close")]
    public async Task<IActionResult> Close(string id, [FromBody] CloseWeeklyPlanRequest req) =>
        Ok(new ApiResponse<WeeklyPlanDto>(true,
            await sender.Send(new CloseWeeklyPlanCommand(user.UserId, id, req.Review))));
}

public record CloseWeeklyPlanRequest(string? Review);
