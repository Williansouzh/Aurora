using Aurora.API.Authorization;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Transfers.Common;
using Aurora.Application.Features.Transfers.Create;
using Aurora.Application.Features.Transfers.Delete;
using Aurora.Application.Features.Transfers.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, RequireModule(ModuleKeys.Finances), Route("api/transfers")]
public class TransfersController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTransfers([FromQuery] int? month, [FromQuery] int? year) =>
        Ok(new ApiResponse<List<TransferDto>>(true, await sender.Send(new GetTransfersQuery(user.UserId, month, year))));

    [HttpPost]
    public async Task<IActionResult> CreateTransfer(CreateTransferCommand req) =>
        Ok(new ApiResponse<TransferDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransfer(string id)
    {
        await sender.Send(new DeleteTransferCommand(user.UserId, id));
        return Ok(new ApiResponse<string>(true, "deleted"));
    }
}
