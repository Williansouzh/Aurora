using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Today.Backlog;
using Aurora.Application.Features.Today.Common;
using Aurora.Application.Features.Today.Complete;
using Aurora.Application.Features.Today.Create;
using Aurora.Application.Features.Today.Delete;
using Aurora.Application.Features.Today.GetToday;
using Aurora.Application.Features.Today.Reopen;
using Aurora.Application.Features.Today.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, Route("api/today")]
public class TodayController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetToday([FromQuery] DateTime? date) =>
        Ok(new ApiResponse<TodayResponse>(true, await sender.Send(new GetTodayTasksQuery(user.UserId, date))));

    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateDailyTaskCommand req) =>
        Ok(new ApiResponse<DailyTaskDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(string id, UpdateDailyTaskCommand req) =>
        Ok(new ApiResponse<DailyTaskDto>(true, await sender.Send(req with { UserId = user.UserId, Id = id })));

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(string id) =>
        Ok(new ApiResponse<DailyTaskDto>(true, await sender.Send(new CompleteDailyTaskCommand(user.UserId, id))));

    [HttpPatch("{id}/reopen")]
    public async Task<IActionResult> Reopen(string id) =>
        Ok(new ApiResponse<DailyTaskDto>(true, await sender.Send(new ReopenDailyTaskCommand(user.UserId, id))));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await sender.Send(new DeleteDailyTaskCommand(user.UserId, id));
        return Ok(new ApiResponse<string>(true, "deleted"));
    }

    // Backlog
    [HttpGet("backlog")]
    public async Task<IActionResult> GetBacklog() =>
        Ok(new ApiResponse<List<DailyTaskDto>>(true, await sender.Send(new GetBacklogQuery(user.UserId))));

    [HttpPost("backlog")]
    public async Task<IActionResult> AddToBacklog(AddToBacklogCommand req) =>
        Ok(new ApiResponse<DailyTaskDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpPatch("{id}/move-to-today")]
    public async Task<IActionResult> MoveToToday(string id) =>
        Ok(new ApiResponse<DailyTaskDto>(true, await sender.Send(new MoveToTodayCommand(user.UserId, id))));
}
