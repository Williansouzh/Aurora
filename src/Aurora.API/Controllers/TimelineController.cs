using Aurora.API.Authorization;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Timeline.Common;
using Aurora.Application.Features.Timeline.CreatePost;
using Aurora.Application.Features.Timeline.Favorite;
using Aurora.Application.Features.Timeline.GetPaged;
using Aurora.Application.Features.Timeline.Hide;
using Aurora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, RequireModule(ModuleKeys.Timeline), Route("api/timeline")]
public class TimelineController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTimeline(
        [FromQuery] TimelineEventType? type,
        [FromQuery] LifeArea? area,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool favoritesOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20) =>
        Ok(new ApiResponse<PagedResultDto<TimelineEventDto>>(true,
            await sender.Send(new GetTimelineQuery(user.UserId, type, area, from, to, favoritesOnly, page, pageSize))));

    [HttpPost]
    public async Task<IActionResult> CreatePost(CreateTimelinePostCommand req) =>
        Ok(new ApiResponse<TimelineEventDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpPatch("{id}/hide")]
    public async Task<IActionResult> Hide(string id) =>
        Ok(new ApiResponse<TimelineEventDto>(true, await sender.Send(new HideTimelineEventCommand(user.UserId, id, true))));

    [HttpPatch("{id}/unhide")]
    public async Task<IActionResult> Unhide(string id) =>
        Ok(new ApiResponse<TimelineEventDto>(true, await sender.Send(new HideTimelineEventCommand(user.UserId, id, false))));

    [HttpPatch("{id}/favorite")]
    public async Task<IActionResult> Favorite(string id) =>
        Ok(new ApiResponse<TimelineEventDto>(true, await sender.Send(new FavoriteTimelineEventCommand(user.UserId, id, true))));

    [HttpPatch("{id}/unfavorite")]
    public async Task<IActionResult> Unfavorite(string id) =>
        Ok(new ApiResponse<TimelineEventDto>(true, await sender.Send(new FavoriteTimelineEventCommand(user.UserId, id, false))));
}
