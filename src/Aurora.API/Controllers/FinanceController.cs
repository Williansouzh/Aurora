using Aurora.Application.Common;
using Aurora.Application.Features;
using Aurora.Application.Interfaces;
using Aurora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;
[ApiController, Authorize]
public class FinanceController(ISender sender, IUserContext user): ControllerBase {
 [HttpGet("api/accounts")] public async Task<IActionResult> Accounts()=>Ok(new ApiResponse<List<AccountDto>>(true, await sender.Send(new GetAccountsQuery(user.UserId))));
 [HttpGet("api/accounts/{id}")] public async Task<IActionResult> AccountById(string id)=>Ok(new ApiResponse<AccountDto>(true, await sender.Send(new GetAccountByIdQuery(user.UserId,id))));
 [HttpPost("api/accounts")] public async Task<IActionResult> CreateAccount(CreateAccountCommand req)=>Ok(new ApiResponse<AccountDto>(true, await sender.Send(req with { UserId=user.UserId })));
 [HttpPut("api/accounts/{id}")] public async Task<IActionResult> UpdateAccount(string id, UpdateAccountCommand req)=>Ok(new ApiResponse<AccountDto>(true, await sender.Send(req with { UserId=user.UserId, Id=id })));
 [HttpPatch("api/accounts/{id}/archive")] public async Task<IActionResult> ArchiveAccount(string id){ await sender.Send(new ArchiveAccountCommand(user.UserId,id)); return Ok(new ApiResponse<string>(true,"archived")); }
 [HttpDelete("api/accounts/{id}")] public async Task<IActionResult> DeleteAccount(string id){ await sender.Send(new DeleteAccountCommand(user.UserId,id)); return Ok(new ApiResponse<string>(true,"deleted")); }

 [HttpGet("api/categories")] public async Task<IActionResult> Categories()=>Ok(new ApiResponse<List<CategoryDto>>(true, await sender.Send(new GetCategoriesQuery(user.UserId))));
 [HttpPost("api/categories")] public async Task<IActionResult> CreateCategory(CreateCategoryCommand req)=>Ok(new ApiResponse<CategoryDto>(true, await sender.Send(req with { UserId=user.UserId })));
 [HttpPut("api/categories/{id}")] public async Task<IActionResult> UpdateCategory(string id, UpdateCategoryCommand req)=>Ok(new ApiResponse<CategoryDto>(true, await sender.Send(req with { UserId=user.UserId, Id=id })));
 [HttpDelete("api/categories/{id}")] public async Task<IActionResult> DeleteCategory(string id){ await sender.Send(new DeleteCategoryCommand(user.UserId,id)); return Ok(new ApiResponse<string>(true,"deleted")); }

 [HttpGet("api/transactions")] public async Task<IActionResult> Transactions([FromQuery]int? month,[FromQuery]int? year,[FromQuery]TransactionType? type,[FromQuery]TransactionStatus? status,[FromQuery]string? categoryId,[FromQuery]string? accountId)=>Ok(new ApiResponse<List<TransactionDto>>(true, await sender.Send(new GetTransactionsQuery(user.UserId,month,year,type,status,categoryId,accountId))));
 [HttpGet("api/transactions/{id}")] public async Task<IActionResult> TransactionById(string id)=>Ok(new ApiResponse<TransactionDto>(true, await sender.Send(new GetTransactionByIdQuery(user.UserId,id))));
 [HttpPost("api/transactions")] public async Task<IActionResult> CreateTransaction(CreateTransactionCommand req)=>Ok(new ApiResponse<TransactionDto>(true, await sender.Send(req with { UserId=user.UserId })));
 [HttpPut("api/transactions/{id}")] public async Task<IActionResult> UpdateTransaction(string id, UpdateTransactionCommand req)=>Ok(new ApiResponse<TransactionDto>(true, await sender.Send(req with { UserId=user.UserId, Id=id })));
 [HttpDelete("api/transactions/{id}")] public async Task<IActionResult> DeleteTransaction(string id){ await sender.Send(new DeleteTransactionCommand(user.UserId,id)); return Ok(new ApiResponse<string>(true,"deleted")); }
 [HttpPatch("api/transactions/{id}/mark-as-paid")] public async Task<IActionResult> MarkAsPaid(string id)=>Ok(new ApiResponse<TransactionDto>(true, await sender.Send(new MarkTransactionAsPaidCommand(user.UserId,id))));
 [HttpPatch("api/transactions/{id}/mark-as-pending")] public async Task<IActionResult> MarkAsPending(string id)=>Ok(new ApiResponse<TransactionDto>(true, await sender.Send(new MarkTransactionAsPendingCommand(user.UserId,id))));

 [HttpGet("api/dashboard/monthly-summary")] public async Task<IActionResult> Summary([FromQuery]int month,[FromQuery]int year)=>Ok(new ApiResponse<MonthlySummaryDto>(true, await sender.Send(new GetMonthlySummaryQuery(user.UserId,month,year))));
 [HttpGet("api/dashboard/category-expenses")] public async Task<IActionResult> CategoryExpenses([FromQuery]int month,[FromQuery]int year)=>Ok(new ApiResponse<List<CategoryExpenseDto>>(true, await sender.Send(new GetCategoryExpensesQuery(user.UserId,month,year))));
 [HttpGet("api/dashboard/cash-flow")] public async Task<IActionResult> CashFlow([FromQuery]int year)=>Ok(new ApiResponse<List<CashFlowItemDto>>(true, await sender.Send(new GetCashFlowQuery(user.UserId,year))));
}
