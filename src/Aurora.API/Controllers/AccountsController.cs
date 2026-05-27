using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Accounts.Archive;
using Aurora.Application.Features.Accounts.Common;
using Aurora.Application.Features.Accounts.Create;
using Aurora.Application.Features.Accounts.Delete;
using Aurora.Application.Features.Accounts.GetAll;
using Aurora.Application.Features.Accounts.GetById;
using Aurora.Application.Features.Accounts.Update;
using Aurora.Application.Features.CreditCardInvoices.Common;
using Aurora.Application.Features.CreditCardInvoices.GetByAccount;
using Aurora.Application.Features.CreditCardInvoices.GetByPeriod;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, Route("api/accounts")]
public class AccountsController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Accounts() =>
        Ok(new ApiResponse<List<AccountDto>>(true, await sender.Send(new GetAccountsQuery(user.UserId))));

    [HttpGet("{id}")]
    public async Task<IActionResult> AccountById(string id) =>
        Ok(new ApiResponse<AccountDto>(true, await sender.Send(new GetAccountByIdQuery(user.UserId, id))));

    [HttpGet("{id}/invoices")]
    public async Task<IActionResult> AccountInvoices(string id) =>
        Ok(new ApiResponse<List<CreditCardInvoiceDto>>(
            true, await sender.Send(new GetCreditCardInvoicesQuery(user.UserId, id))));

    [HttpGet("{id}/invoices/{month:int}/{year:int}")]
    public async Task<IActionResult> AccountInvoice(string id, int month, int year) =>
        Ok(new ApiResponse<CreditCardInvoiceDto>(
            true, await sender.Send(new GetCreditCardInvoiceQuery(user.UserId, id, month, year))));

    [HttpPost]
    public async Task<IActionResult> CreateAccount(CreateAccountCommand req) =>
        Ok(new ApiResponse<AccountDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(string id, UpdateAccountCommand req) =>
        Ok(new ApiResponse<AccountDto>(true, await sender.Send(req with { UserId = user.UserId, Id = id })));

    [HttpPatch("{id}/archive")]
    public async Task<IActionResult> ArchiveAccount(string id)
    {
        await sender.Send(new ArchiveAccountCommand(user.UserId, id));
        return Ok(new ApiResponse<string>(true, "archived"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(string id)
    {
        await sender.Send(new DeleteAccountCommand(user.UserId, id));
        return Ok(new ApiResponse<string>(true, "deleted"));
    }
}
