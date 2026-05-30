using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Goals.ChangeStatus;
using Aurora.Application.Features.Goals.Common;
using Aurora.Application.Features.Goals.Create;
using Aurora.Application.Features.Goals.Delete;
using Aurora.Application.Features.Goals.GetAll;
using Aurora.Application.Features.Goals.GetById;
using Aurora.Application.Features.Goals.Milestones;
using Aurora.Application.Features.Goals.Update;
using Aurora.Application.Features.Goals.UpdateProgress;
using Aurora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, Route("api/goals")]
public class GoalsController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GoalStatus? status) =>
        Ok(new ApiResponse<List<GoalDto>>(true, await sender.Send(new GetGoalsQuery(user.UserId, status))));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id) =>
        Ok(new ApiResponse<GoalDto>(true, await sender.Send(new GetGoalByIdQuery(user.UserId, id))));

    [HttpPost]
    public async Task<IActionResult> Create(CreateGoalCommand req) =>
        Ok(new ApiResponse<GoalDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateGoalCommand req) =>
        Ok(new ApiResponse<GoalDto>(true, await sender.Send(req with { UserId = user.UserId, Id = id })));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await sender.Send(new DeleteGoalCommand(user.UserId, id));
        return Ok(new ApiResponse<string>(true, "deleted"));
    }

    [HttpPatch("{id}/progress")]
    public async Task<IActionResult> UpdateProgress(string id, [FromBody] UpdateProgressRequest req) =>
        Ok(new ApiResponse<GoalDto>(true,
            await sender.Send(new UpdateGoalProgressCommand(user.UserId, id, req.CurrentValue))));

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeStatus(string id, [FromBody] ChangeStatusRequest req) =>
        Ok(new ApiResponse<GoalDto>(true,
            await sender.Send(new ChangeGoalStatusCommand(user.UserId, id, req.Action, req.Reason))));

    // Milestones
    [HttpPost("{id}/milestones")]
    public async Task<IActionResult> AddMilestone(string id, [FromBody] AddMilestoneRequest req) =>
        Ok(new ApiResponse<GoalDto>(true,
            await sender.Send(new AddMilestoneCommand(user.UserId, id, req.Title, req.IsRequired))));

    [HttpPatch("{id}/milestones/{milestoneId}/complete")]
    public async Task<IActionResult> CompleteMilestone(string id, string milestoneId) =>
        Ok(new ApiResponse<GoalDto>(true,
            await sender.Send(new CompleteMilestoneCommand(user.UserId, id, milestoneId))));

    [HttpPatch("{id}/milestones/{milestoneId}/reopen")]
    public async Task<IActionResult> ReopenMilestone(string id, string milestoneId) =>
        Ok(new ApiResponse<GoalDto>(true,
            await sender.Send(new ReopenMilestoneCommand(user.UserId, id, milestoneId))));

    [HttpDelete("{id}/milestones/{milestoneId}")]
    public async Task<IActionResult> DeleteMilestone(string id, string milestoneId) =>
        Ok(new ApiResponse<GoalDto>(true,
            await sender.Send(new DeleteMilestoneCommand(user.UserId, id, milestoneId))));
}

public record UpdateProgressRequest(decimal CurrentValue);
public record ChangeStatusRequest(string Action, string? Reason);
public record AddMilestoneRequest(string Title, bool IsRequired);
