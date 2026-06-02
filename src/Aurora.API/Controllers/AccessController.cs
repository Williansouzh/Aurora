using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Access.Common;
using Aurora.Application.Features.Access.GetMyAccess;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, Route("api/access")]
public class AccessController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> Me() =>
        Ok(new ApiResponse<AccessSnapshotDto>(true, await sender.Send(new GetMyAccessQuery(user.UserId))));
}
