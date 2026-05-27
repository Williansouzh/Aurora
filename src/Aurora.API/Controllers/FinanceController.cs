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
 [HttpGet("api/accounts/{id}/invoices")] public async Task<IActionResult> AccountInvoices(string id)=>Ok(new ApiResponse<List<CreditCardInvoiceDto>>(true, await sender.Send(new GetCreditCardInvoicesQuery(user.UserId,id))));
 [HttpGet("api/accounts/{id}/invoices/{month:int}/{year:int}")] public async Task<IActionResult> AccountInvoice(string id,int month,int year)=>Ok(new ApiResponse<CreditCardInvoiceDto>(true, await sender.Send(new GetCreditCardInvoiceQuery(user.UserId,id,month,year))));
 [HttpPost("api/accounts")] public async Task<IActionResult> CreateAccount(CreateAccountCommand req)=>Ok(new ApiResponse<AccountDto>(true, await sender.Send(req with { UserId=user.UserId })));
 [HttpPut("api/accounts/{id}")] public async Task<IActionResult> UpdateAccount(string id, UpdateAccountCommand req)=>Ok(new ApiResponse<AccountDto>(true, await sender.Send(req with { UserId=user.UserId, Id=id })));
 [HttpPatch("api/accounts/{id}/archive")] public async Task<IActionResult> ArchiveAccount(string id){ await sender.Send(new ArchiveAccountCommand(user.UserId,id)); return Ok(new ApiResponse<string>(true,"archived")); }
 [HttpDelete("api/accounts/{id}")] public async Task<IActionResult> DeleteAccount(string id){ await sender.Send(new DeleteAccountCommand(user.UserId,id)); return Ok(new ApiResponse<string>(true,"deleted")); }

 [HttpGet("api/categories")] public async Task<IActionResult> Categories()=>Ok(new ApiResponse<List<CategoryDto>>(true, await sender.Send(new GetCategoriesQuery(user.UserId))));
 [HttpPost("api/categories")] public async Task<IActionResult> CreateCategory(CreateCategoryCommand req)=>Ok(new ApiResponse<CategoryDto>(true, await sender.Send(req with { UserId=user.UserId })));
 [HttpPut("api/categories/{id}")] public async Task<IActionResult> UpdateCategory(string id, UpdateCategoryCommand req)=>Ok(new ApiResponse<CategoryDto>(true, await sender.Send(req with { UserId=user.UserId, Id=id })));
 [HttpDelete("api/categories/{id}")] public async Task<IActionResult> DeleteCategory(string id){ await sender.Send(new DeleteCategoryCommand(user.UserId,id)); return Ok(new ApiResponse<string>(true,"deleted")); }

 [HttpGet("api/transactions")] public async Task<IActionResult> Transactions([FromQuery]int? month,[FromQuery]int? year,[FromQuery]TransactionType? type,[FromQuery]TransactionStatus? status,[FromQuery]string? categoryId,[FromQuery]string? accountId,[FromQuery]string? search,[FromQuery]DateTime? dateFrom,[FromQuery]DateTime? dateTo,[FromQuery]string? accountIds,[FromQuery]string? categoryIds,[FromQuery]decimal? minAmount,[FromQuery]decimal? maxAmount,[FromQuery]int page=1,[FromQuery]int pageSize=20)=>Ok(new ApiResponse<PagedResultDto<TransactionDto>>(true, await sender.Send(new GetTransactionsQuery(user.UserId,month,year,type,status,categoryId,accountId,search,dateFrom,dateTo,SplitIds(accountIds),SplitIds(categoryIds),minAmount,maxAmount,page,pageSize))));
 [HttpGet("api/transactions/export/csv")] public async Task<IActionResult> ExportTransactionsCsv([FromQuery]int? month,[FromQuery]int? year,[FromQuery]TransactionType? type,[FromQuery]TransactionStatus? status,[FromQuery]string? categoryId,[FromQuery]string? accountId,[FromQuery]DateTime? dateFrom,[FromQuery]DateTime? dateTo){ var file=await sender.Send(new ExportTransactionsCsvQuery(user.UserId,month,year,type,status,categoryId,accountId,dateFrom,dateTo)); var bytes=new byte[]{0xEF,0xBB,0xBF}.Concat(System.Text.Encoding.UTF8.GetBytes(file.Content)).ToArray(); return File(bytes,"text/csv",file.FileName); }
 [HttpGet("api/transactions/{id}")] public async Task<IActionResult> TransactionById(string id)=>Ok(new ApiResponse<TransactionDto>(true, await sender.Send(new GetTransactionByIdQuery(user.UserId,id))));
 [HttpPost("api/transactions")] public async Task<IActionResult> CreateTransaction(CreateTransactionCommand req)=>Ok(new ApiResponse<TransactionDto>(true, await sender.Send(req with { UserId=user.UserId })));
 [HttpPut("api/transactions/{id}")] public async Task<IActionResult> UpdateTransaction(string id, UpdateTransactionCommand req)=>Ok(new ApiResponse<TransactionDto>(true, await sender.Send(req with { UserId=user.UserId, Id=id })));
 [HttpDelete("api/transactions/{id}")] public async Task<IActionResult> DeleteTransaction(string id){ await sender.Send(new DeleteTransactionCommand(user.UserId,id)); return Ok(new ApiResponse<string>(true,"deleted")); }
 [HttpPatch("api/transactions/{id}/recurrence")] public async Task<IActionResult> UpdateRecurringTransaction(string id, UpdateRecurringTransactionCommand req)=>Ok(new ApiResponse<List<TransactionDto>>(true, await sender.Send(req with { UserId=user.UserId, Id=id })));
 [HttpDelete("api/transactions/{id}/recurrence")] public async Task<IActionResult> DeleteRecurringTransaction(string id,[FromBody] RecurrenceScopeRequest req){ await sender.Send(new DeleteRecurringTransactionCommand(user.UserId,id,req.Scope)); return Ok(new ApiResponse<string>(true,"deleted")); }
 [HttpPatch("api/transactions/{id}/mark-as-paid")] public async Task<IActionResult> MarkAsPaid(string id)=>Ok(new ApiResponse<TransactionDto>(true, await sender.Send(new MarkTransactionAsPaidCommand(user.UserId,id))));
 [HttpPatch("api/transactions/{id}/mark-as-pending")] public async Task<IActionResult> MarkAsPending(string id)=>Ok(new ApiResponse<TransactionDto>(true, await sender.Send(new MarkTransactionAsPendingCommand(user.UserId,id))));
 [HttpPost("api/invoices/{id}/pay")] public async Task<IActionResult> PayInvoice(string id, PayInvoiceRequest req)=>Ok(new ApiResponse<CreditCardInvoiceDto>(true, await sender.Send(new PayCreditCardInvoiceCommand(user.UserId,id,req.SourceAccountId,req.Amount))));

 [HttpGet("api/dashboard/monthly-summary")] public async Task<IActionResult> Summary([FromQuery]int month,[FromQuery]int year)=>Ok(new ApiResponse<MonthlySummaryDto>(true, await sender.Send(new GetMonthlySummaryQuery(user.UserId,month,year))));
 [HttpGet("api/dashboard/upcoming-dues")] public async Task<IActionResult> UpcomingDues([FromQuery]int days=7,[FromQuery]string status="pending")=>Ok(new ApiResponse<List<UpcomingDueDto>>(true, await sender.Send(new UpcomingDuesQuery(user.UserId,days,status))));
 [HttpGet("api/dashboard/category-expenses")] public async Task<IActionResult> CategoryExpenses([FromQuery]int month,[FromQuery]int year)=>Ok(new ApiResponse<List<CategoryExpenseDto>>(true, await sender.Send(new GetCategoryExpensesQuery(user.UserId,month,year))));
 [HttpGet("api/dashboard/cash-flow")] public async Task<IActionResult> CashFlow([FromQuery]int year)=>Ok(new ApiResponse<List<CashFlowItemDto>>(true, await sender.Send(new GetCashFlowQuery(user.UserId,year))));
 [HttpGet("api/dashboard/financing-summary")] public async Task<IActionResult> FinancingSummary()=>Ok(new ApiResponse<FinancingSummaryDto>(true, await sender.Send(new GetFinancingSummaryQuery(user.UserId))));

 static List<string>? SplitIds(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Split(',',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).ToList();
}
public record PayInvoiceRequest(string SourceAccountId,decimal Amount);
public record RecurrenceScopeRequest(string Scope);
