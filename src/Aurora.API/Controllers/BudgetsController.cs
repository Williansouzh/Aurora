using Aurora.Application.Common;
using Aurora.Application.Features;
using Aurora.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize]
public class BudgetsController(ISender sender, IUserContext user): ControllerBase {
 [HttpGet("api/budgets")] public async Task<IActionResult> GetBudgets([FromQuery]int month,[FromQuery]int year)=>Ok(new ApiResponse<List<BudgetDto>>(true, await sender.Send(new GetBudgetsQuery(user.UserId,month,year))));
 [HttpPost("api/budgets")] public async Task<IActionResult> UpsertBudget(UpsertBudgetCommand req)=>Ok(new ApiResponse<BudgetDto>(true, await sender.Send(req with { UserId=user.UserId })));
 [HttpDelete("api/budgets/{id}")] public async Task<IActionResult> DeleteBudget(string id){ await sender.Send(new DeleteBudgetCommand(user.UserId,id)); return Ok(new ApiResponse<string>(true,"deleted")); }
}
