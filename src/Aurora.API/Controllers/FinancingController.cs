using Aurora.Application.Common;
using Aurora.Application.Features;
using Aurora.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Aurora.API.Controllers;

[ApiController, Authorize]
public class FinancingController(ISender sender, IUserContext user): ControllerBase {
 [HttpGet("api/financings")] public async Task<IActionResult> Financings()=>Ok(new ApiResponse<List<FinancingDto>>(true, await sender.Send(new GetFinancingsQuery(user.UserId))));
 [HttpGet("api/financings/{id}")] public async Task<IActionResult> FinancingById(string id)=>Ok(new ApiResponse<FinancingDto>(true, await sender.Send(new GetFinancingByIdQuery(user.UserId,id))));
 [HttpPost("api/financings")] public async Task<IActionResult> CreateFinancing(CreateFinancingCommand req)=>Ok(new ApiResponse<FinancingDto>(true, await sender.Send(req with { UserId=user.UserId })));
 [HttpPut("api/financings/{id}")] public async Task<IActionResult> UpdateFinancing(string id, UpdateFinancingCommand req)=>Ok(new ApiResponse<FinancingDto>(true, await sender.Send(req with { UserId=user.UserId, Id=id })));
 [HttpPost("api/financings/simulate")] public async Task<IActionResult> Simulate(SimulateFinancingCommand req)=>Ok(new ApiResponse<FinancingSimulationDto>(true, await sender.Send(req)));
 [HttpPost("api/financings/compare")] public async Task<IActionResult> Compare(CompareFinancingCommand req)=>Ok(new ApiResponse<FinancingComparisonDto>(true, await sender.Send(req)));
 [HttpPost("api/financings/{id}/simulate-extra-amortization")] public async Task<IActionResult> SimulateExtraAmortization(string id, SimulateExtraAmortizationCommand req)=>Ok(new ApiResponse<ExtraAmortizationSimulationDto>(true, await sender.Send(req with { UserId=user.UserId, FinancingId=id })));
 [HttpPatch("api/financings/{id}/installments/{number}/mark-as-paid")] public async Task<IActionResult> MarkInstallmentAsPaid(string id, int number, [FromBody(EmptyBodyBehavior=EmptyBodyBehavior.Allow)] MarkAsPaidRequest? req)=>Ok(new ApiResponse<FinancingDto>(true, await sender.Send(new MarkFinancingInstallmentAsPaidCommand(user.UserId,id,number,req?.PaidAmount,req?.PaidAt,req?.LinkedTransactionId))));
 [HttpPatch("api/financings/{id}/installments/{number}/link-transaction")] public async Task<IActionResult> LinkTransaction(string id, int number, LinkTransactionRequest req)=>Ok(new ApiResponse<FinancingDto>(true, await sender.Send(new LinkTransactionCommand(user.UserId,id,number,req.TransactionId))));
 [HttpDelete("api/financings/{id}")] public async Task<IActionResult> DeleteFinancing(string id){ await sender.Send(new DeleteFinancingCommand(user.UserId,id)); return Ok(new ApiResponse<string>(true,"deleted")); }
}
public record MarkAsPaidRequest(decimal? PaidAmount=null, DateTime? PaidAt=null, string? LinkedTransactionId=null);
public record LinkTransactionRequest(string TransactionId);
