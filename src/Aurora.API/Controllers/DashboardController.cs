using Aurora.API.Authorization;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Dashboard.CashFlow;
using Aurora.Application.Features.Dashboard.CategoryExpenses;
using Aurora.Application.Features.Dashboard.Common;
using Aurora.Application.Features.Dashboard.MonthlySummary;
using Aurora.Application.Features.Dashboard.UpcomingDues;
using Aurora.Application.Features.Financings.Common;
using Aurora.Application.Features.Financings.Summary;
using Aurora.Application.Features.Transactions.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, RequireModule(ModuleKeys.Finances), Route("api/dashboard")]
public class DashboardController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet("monthly-summary")]
    public async Task<IActionResult> Summary([FromQuery] int month, [FromQuery] int year) =>
        Ok(new ApiResponse<MonthlySummaryDto>(
            true, await sender.Send(new GetMonthlySummaryQuery(user.UserId, month, year))));

    [HttpGet("upcoming-dues")]
    public async Task<IActionResult> UpcomingDues([FromQuery] int days = 7, [FromQuery] string status = "pending") =>
        Ok(new ApiResponse<List<UpcomingDueDto>>(
            true, await sender.Send(new UpcomingDuesQuery(user.UserId, days, status))));

    [HttpGet("category-expenses")]
    public async Task<IActionResult> CategoryExpenses([FromQuery] int month, [FromQuery] int year) =>
        Ok(new ApiResponse<List<CategoryExpenseDto>>(
            true, await sender.Send(new GetCategoryExpensesQuery(user.UserId, month, year))));

    [HttpGet("cash-flow")]
    public async Task<IActionResult> CashFlow([FromQuery] int year) =>
        Ok(new ApiResponse<List<CashFlowItemDto>>(
            true, await sender.Send(new GetCashFlowQuery(user.UserId, year))));

    [HttpGet("financing-summary")]
    public async Task<IActionResult> FinancingSummary() =>
        Ok(new ApiResponse<FinancingSummaryDto>(true, await sender.Send(new GetFinancingSummaryQuery(user.UserId))));
}
