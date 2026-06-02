using Aurora.API.Authorization;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.CreditCardInvoices.Common;
using Aurora.Application.Features.CreditCardInvoices.Pay;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, RequireModule(ModuleKeys.Finances), Route("api/invoices")]
public class InvoicesController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpPost("{id}/pay")]
    public async Task<IActionResult> PayInvoice(string id, PayInvoiceRequest req) =>
        Ok(new ApiResponse<CreditCardInvoiceDto>(true,
            await sender.Send(new PayCreditCardInvoiceCommand(user.UserId, id, req.SourceAccountId, req.Amount))));
}

public record PayInvoiceRequest(string SourceAccountId, decimal Amount);
