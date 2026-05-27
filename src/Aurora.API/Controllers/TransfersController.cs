using Aurora.Application.Common;
using Aurora.Application.Features;
using Aurora.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize]
public class TransfersController(ISender sender, IUserContext user): ControllerBase {
 [HttpGet("api/transfers")] public async Task<IActionResult> GetTransfers([FromQuery]int? month,[FromQuery]int? year)=>Ok(new ApiResponse<List<TransferDto>>(true, await sender.Send(new GetTransfersQuery(user.UserId,month,year))));
 [HttpPost("api/transfers")] public async Task<IActionResult> CreateTransfer(CreateTransferCommand req)=>Ok(new ApiResponse<TransferDto>(true, await sender.Send(req with { UserId=user.UserId })));
 [HttpDelete("api/transfers/{id}")] public async Task<IActionResult> DeleteTransfer(string id){ await sender.Send(new DeleteTransferCommand(user.UserId,id)); return Ok(new ApiResponse<string>(true,"deleted")); }
}
