using Aurora.API.Authorization;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Budgets.Common;
using Aurora.Application.Features.Budgets.Delete;
using Aurora.Application.Features.Budgets.GetAll;
using Aurora.Application.Features.Budgets.Upsert;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, RequireModule(ModuleKeys.Finances), Route("api/budgets")]
public class BudgetsController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBudgets([FromQuery] int month, [FromQuery] int year) =>
        Ok(new ApiResponse<List<BudgetDto>>(true, await sender.Send(new GetBudgetsQuery(user.UserId, month, year))));

    [HttpPost]
    public async Task<IActionResult> UpsertBudget(UpsertBudgetCommand req) =>
        Ok(new ApiResponse<BudgetDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBudget(string id)
    {
        await sender.Send(new DeleteBudgetCommand(user.UserId, id));
        return Ok(new ApiResponse<string>(true, "deleted"));
    }
}
