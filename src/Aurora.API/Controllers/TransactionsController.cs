using System.Text;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Transactions.Common;
using Aurora.Application.Features.Transactions.Create;
using Aurora.Application.Features.Transactions.Delete;
using Aurora.Application.Features.Transactions.DeleteRecurring;
using Aurora.Application.Features.Transactions.ExportCsv;
using Aurora.Application.Features.Transactions.GetById;
using Aurora.Application.Features.Transactions.GetPaged;
using Aurora.Application.Features.Transactions.MarkAsPaid;
using Aurora.Application.Features.Transactions.MarkAsPending;
using Aurora.Application.Features.Transactions.Update;
using Aurora.Application.Features.Transactions.UpdateRecurring;
using Aurora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, Route("api/transactions")]
public class TransactionsController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Transactions(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] TransactionType? type,
        [FromQuery] TransactionStatus? status,
        [FromQuery] string? categoryId,
        [FromQuery] string? accountId,
        [FromQuery] string? search,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? accountIds,
        [FromQuery] string? categoryIds,
        [FromQuery] decimal? minAmount,
        [FromQuery] decimal? maxAmount,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20) =>
        Ok(new ApiResponse<PagedResultDto<TransactionDto>>(true, await sender.Send(new GetTransactionsQuery(
            user.UserId, month, year, type, status, categoryId, accountId,
            search, dateFrom, dateTo, SplitIds(accountIds), SplitIds(categoryIds),
            minAmount, maxAmount, page, pageSize))));

    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportTransactionsCsv(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] TransactionType? type,
        [FromQuery] TransactionStatus? status,
        [FromQuery] string? categoryId,
        [FromQuery] string? accountId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var file = await sender.Send(new ExportTransactionsCsvQuery(
            user.UserId, month, year, type, status, categoryId, accountId, dateFrom, dateTo));

        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var bytes = bom.Concat(Encoding.UTF8.GetBytes(file.Content)).ToArray();
        return File(bytes, "text/csv", file.FileName);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> TransactionById(string id) =>
        Ok(new ApiResponse<TransactionDto>(true, await sender.Send(new GetTransactionByIdQuery(user.UserId, id))));

    [HttpPost]
    public async Task<IActionResult> CreateTransaction(CreateTransactionCommand req) =>
        Ok(new ApiResponse<TransactionDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(string id, UpdateTransactionCommand req) =>
        Ok(new ApiResponse<TransactionDto>(true, await sender.Send(req with { UserId = user.UserId, Id = id })));

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(string id)
    {
        await sender.Send(new DeleteTransactionCommand(user.UserId, id));
        return Ok(new ApiResponse<string>(true, "deleted"));
    }

    [HttpPatch("{id}/recurrence")]
    public async Task<IActionResult> UpdateRecurringTransaction(string id, UpdateRecurringTransactionCommand req) =>
        Ok(new ApiResponse<List<TransactionDto>>(true, await sender.Send(req with { UserId = user.UserId, Id = id })));

    [HttpDelete("{id}/recurrence")]
    public async Task<IActionResult> DeleteRecurringTransaction(string id, [FromBody] RecurrenceScopeRequest req)
    {
        await sender.Send(new DeleteRecurringTransactionCommand(user.UserId, id, req.Scope));
        return Ok(new ApiResponse<string>(true, "deleted"));
    }

    [HttpPatch("{id}/mark-as-paid")]
    public async Task<IActionResult> MarkAsPaid(string id) =>
        Ok(new ApiResponse<TransactionDto>(true, await sender.Send(new MarkTransactionAsPaidCommand(user.UserId, id))));

    [HttpPatch("{id}/mark-as-pending")]
    public async Task<IActionResult> MarkAsPending(string id) =>
        Ok(new ApiResponse<TransactionDto>(true, await sender.Send(new MarkTransactionAsPendingCommand(user.UserId, id))));

    private static List<string>? SplitIds(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}

public record RecurrenceScopeRequest(string Scope);
