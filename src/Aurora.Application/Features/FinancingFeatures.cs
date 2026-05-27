using Aurora.Application.Interfaces;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features;

public record FinancingInstallmentDto(int Number,DateTime DueDate,decimal OpeningBalance,decimal Amortization,decimal Interest,decimal Insurance,decimal Fees,decimal TotalPayment,decimal ClosingBalance,FinancingInstallmentStatus Status,DateTime? PaidAt=null,decimal? PaidAmount=null,string? LinkedTransactionId=null);
public record FinancingDto(string Id,string Name,FinancingType Type,AmortizationSystem AmortizationSystem,FinancingStatus Status,string Institution,decimal AssetValue,decimal DownPayment,decimal FinancedAmount,decimal AnnualInterestRate,decimal MonthlyInsurance,decimal MonthlyFees,decimal? CetAnnualRate,int TermMonths,DateTime FirstDueDate,decimal TotalPayment,decimal TotalInterest,decimal RemainingBalance,decimal PaidPrincipal,decimal PaidInterest,decimal PaidTotal,decimal ProgressPercentage,decimal InterestSharePercentage,int PaidInstallments,int RemainingInstallments,List<FinancingInstallmentDto> Installments,string? LinkedAccountId=null,string? Notes=null,string? PropertyAddress=null,string? PropertyRegistration=null,string? VehicleBrand=null,string? VehicleModel=null,int? VehicleYear=null,string? VehiclePlate=null);
public record FinancingSimulationDto(decimal FinancedAmount,decimal FirstPayment,decimal LastPayment,decimal TotalPayment,decimal TotalInterest,decimal AveragePayment,decimal InterestSharePercentage,List<FinancingInstallmentDto> Installments);
public record FinancingComparisonDto(FinancingSimulationDto Sac,FinancingSimulationDto Price,decimal SacInterestSavings,decimal SacInterestSavingsPercentage,decimal SacTotalSavings,decimal SacTotalSavingsPercentage);
public record ExtraAmortizationSimulationDto(decimal ExtraAmount,decimal OriginalRemainingInterest,decimal NewRemainingInterest,decimal InterestSavings,decimal InterestSavingsPercentage,int OriginalRemainingMonths,int NewRemainingMonths,int MonthsSaved,decimal NewLastPayment,List<FinancingInstallmentDto> Installments);

public record GetFinancingsQuery(string UserId):IRequest<List<FinancingDto>>;
public record GetFinancingByIdQuery(string UserId,string Id):IRequest<FinancingDto>;
public record CreateFinancingCommand(string UserId,string Name,FinancingType Type,AmortizationSystem AmortizationSystem,string Institution,decimal AssetValue,decimal DownPayment,decimal AnnualInterestRate,decimal MonthlyInsurance,decimal MonthlyFees,decimal? CetAnnualRate,int TermMonths,DateTime FirstDueDate,string? LinkedAccountId=null,string? Notes=null,string? PropertyAddress=null,string? PropertyRegistration=null,string? VehicleBrand=null,string? VehicleModel=null,int? VehicleYear=null,string? VehiclePlate=null):IRequest<FinancingDto>;
public record SimulateFinancingCommand(decimal AssetValue,decimal DownPayment,AmortizationSystem AmortizationSystem,decimal AnnualInterestRate,decimal MonthlyInsurance,decimal MonthlyFees,int TermMonths,DateTime FirstDueDate):IRequest<FinancingSimulationDto>;
public record CompareFinancingCommand(decimal AssetValue,decimal DownPayment,decimal AnnualInterestRate,decimal MonthlyInsurance,decimal MonthlyFees,int TermMonths,DateTime FirstDueDate):IRequest<FinancingComparisonDto>;
public record SimulateExtraAmortizationCommand(string UserId,string FinancingId,decimal ExtraAmount):IRequest<ExtraAmortizationSimulationDto>;
public record DeleteFinancingCommand(string UserId,string Id):IRequest;
public record MarkFinancingInstallmentAsPaidCommand(string UserId,string FinancingId,int Number,decimal? PaidAmount=null,DateTime? PaidAt=null,string? LinkedTransactionId=null):IRequest<FinancingDto>;
public record UpdateFinancingCommand(string UserId,string Id,string Name,FinancingType Type,AmortizationSystem AmortizationSystem,string Institution,decimal AssetValue,decimal DownPayment,decimal AnnualInterestRate,decimal MonthlyInsurance,decimal MonthlyFees,decimal? CetAnnualRate,int TermMonths,DateTime FirstDueDate,string? LinkedAccountId=null,string? Notes=null,string? PropertyAddress=null,string? PropertyRegistration=null,string? VehicleBrand=null,string? VehicleModel=null,int? VehicleYear=null,string? VehiclePlate=null):IRequest<FinancingDto>;
public record LinkTransactionCommand(string UserId,string FinancingId,int Number,string TransactionId):IRequest<FinancingDto>;
public record UpcomingInstallmentDto(string FinancingId,string FinancingName,int Number,DateTime DueDate,decimal TotalPayment,FinancingInstallmentStatus Status);
public record FinancingSummaryDto(int ActiveCount,decimal TotalRemainingBalance,decimal TotalMonthlyPayment,decimal TotalInterestRemaining,decimal OverallProgress,List<UpcomingInstallmentDto> UpcomingInstallments);
public record GetFinancingSummaryQuery(string UserId):IRequest<FinancingSummaryDto>;

public static class FinancingMapper {
 public static FinancingInstallmentDto ToDto(this FinancingInstallment x)=>new(x.Number,x.DueDate,x.OpeningBalance,x.Amortization,x.Interest,x.Insurance,x.Fees,x.TotalPayment,x.ClosingBalance,x.Status,x.PaidAt,x.PaidAmount,x.LinkedTransactionId);
 public static FinancingDto ToDto(this Financing x){
  var total=x.Installments.Sum(i=>i.TotalPayment);
  var totalInterest=x.Installments.Sum(i=>i.Interest);
  var paid=x.Installments.Count(i=>i.Status==FinancingInstallmentStatus.Paid);
  var remaining=x.Installments.Count-paid;
  var paidRows=x.Installments.Where(i=>i.Status==FinancingInstallmentStatus.Paid).ToList();
  var paidPrincipal=paidRows.Sum(i=>i.Amortization);
  var paidInterest=paidRows.Sum(i=>i.Interest);
  var paidTotal=paidRows.Sum(i=>i.TotalPayment);
  var remainingBalance=x.Installments.LastOrDefault(i=>i.Status==FinancingInstallmentStatus.Paid)?.ClosingBalance ?? x.FinancedAmount;
  if(paid==x.Installments.Count&&x.Installments.Count>0) remainingBalance=0;
  var progress=x.FinancedAmount==0?0:Math.Round((paidPrincipal/x.FinancedAmount)*100,2);
  var interestShare=total==0?0:Math.Round((totalInterest/total)*100,2);
  return new(x.Id,x.Name,x.Type,x.AmortizationSystem,x.Status,x.Institution,x.AssetValue,x.DownPayment,x.FinancedAmount,x.AnnualInterestRate,x.MonthlyInsurance,x.MonthlyFees,x.CetAnnualRate,x.TermMonths,x.FirstDueDate,total,totalInterest,remainingBalance,paidPrincipal,paidInterest,paidTotal,progress,interestShare,paid,remaining,x.Installments.Select(i=>i.ToDto()).ToList(),x.LinkedAccountId,x.Notes,x.PropertyAddress,x.PropertyRegistration,x.VehicleBrand,x.VehicleModel,x.VehicleYear,x.VehiclePlate);
 }
}

public static class FinancingCalculator {
 public static FinancingSimulationDto Simulate(decimal assetValue,decimal downPayment,AmortizationSystem system,decimal annualInterestRate,decimal monthlyInsurance,decimal monthlyFees,int termMonths,DateTime firstDueDate){
  var financedAmount=assetValue-downPayment;
  var installments=BuildSchedule(financedAmount,system,annualInterestRate,monthlyInsurance,monthlyFees,termMonths,firstDueDate);
  var total=installments.Sum(i=>i.TotalPayment);
  var totalInterest=installments.Sum(i=>i.Interest);
  var interestShare=total==0?0:Math.Round((totalInterest/total)*100,2);
  return new FinancingSimulationDto(financedAmount,installments.First().TotalPayment,installments.Last().TotalPayment,total,totalInterest,total/termMonths,interestShare,installments.Select(i=>i.ToDto()).ToList());
 }

 public static List<FinancingInstallment> BuildSchedule(decimal financedAmount,AmortizationSystem system,decimal annualInterestRate,decimal monthlyInsurance,decimal monthlyFees,int termMonths,DateTime firstDueDate){
  if(financedAmount<=0) throw new ValidationException("Valor financiado deve ser positivo");
  if(termMonths<=0) throw new ValidationException("Prazo deve ser positivo");
  if(annualInterestRate<0) throw new ValidationException("Taxa de juros nao pode ser negativa");
  var monthlyRate=annualInterestRate/100m/12m;
  return system==AmortizationSystem.SAC
   ? BuildSac(financedAmount,monthlyRate,monthlyInsurance,monthlyFees,termMonths,firstDueDate)
   : BuildPrice(financedAmount,monthlyRate,monthlyInsurance,monthlyFees,termMonths,firstDueDate);
 }

 static List<FinancingInstallment> BuildSac(decimal principal,decimal monthlyRate,decimal insurance,decimal fees,int termMonths,DateTime firstDueDate){
  var rows=new List<FinancingInstallment>();
  var balance=principal;
  var amortization=Round(principal/termMonths);
  for(var n=1;n<=termMonths;n++){
   var opening=balance;
   var interest=Round(opening*monthlyRate);
   var currentAmortization=n==termMonths?opening:amortization;
   balance=Round(opening-currentAmortization);
   rows.Add(Row(n,firstDueDate,opening,currentAmortization,interest,insurance,fees,balance));
  }
  return rows;
 }

 static List<FinancingInstallment> BuildPrice(decimal principal,decimal monthlyRate,decimal insurance,decimal fees,int termMonths,DateTime firstDueDate){
  var rows=new List<FinancingInstallment>();
  var balance=principal;
  var basePayment=monthlyRate==0 ? principal/termMonths : principal*monthlyRate/(1-(decimal)Math.Pow((double)(1+monthlyRate),-termMonths));
  basePayment=Round(basePayment);
  for(var n=1;n<=termMonths;n++){
   var opening=balance;
   var interest=Round(opening*monthlyRate);
   var amortization=n==termMonths?opening:Round(basePayment-interest);
   balance=Round(opening-amortization);
   rows.Add(Row(n,firstDueDate,opening,amortization,interest,insurance,fees,balance));
  }
  return rows;
 }

 static FinancingInstallment Row(int number,DateTime firstDueDate,decimal opening,decimal amortization,decimal interest,decimal insurance,decimal fees,decimal closing)=>new(){Number=number,DueDate=firstDueDate.Date.AddMonths(number-1),OpeningBalance=opening,Amortization=amortization,Interest=interest,Insurance=insurance,Fees=fees,TotalPayment=Round(amortization+interest+insurance+fees),ClosingBalance=closing,Status=FinancingInstallmentStatus.Planned};
 static decimal Round(decimal value)=>Math.Round(value,2,MidpointRounding.AwayFromZero);
}

public class GetFinancingsHandler(IFinancingRepository repo):IRequestHandler<GetFinancingsQuery,List<FinancingDto>>{public async Task<List<FinancingDto>> Handle(GetFinancingsQuery q,CancellationToken ct)=>(await repo.GetByUserAsync(q.UserId)).Select(x=>x.ToDto()).ToList();}
public class GetFinancingByIdHandler(IFinancingRepository repo):IRequestHandler<GetFinancingByIdQuery,FinancingDto>{public async Task<FinancingDto> Handle(GetFinancingByIdQuery q,CancellationToken ct)=>(await repo.GetByIdAsync(q.Id,q.UserId)??throw new NotFoundException("Financiamento nao encontrado")).ToDto();}
public class SimulateFinancingHandler:IRequestHandler<SimulateFinancingCommand,FinancingSimulationDto>{public Task<FinancingSimulationDto> Handle(SimulateFinancingCommand c,CancellationToken ct)=>Task.FromResult(FinancingCalculator.Simulate(c.AssetValue,c.DownPayment,c.AmortizationSystem,c.AnnualInterestRate,c.MonthlyInsurance,c.MonthlyFees,c.TermMonths,c.FirstDueDate));}
public class CompareFinancingHandler:IRequestHandler<CompareFinancingCommand,FinancingComparisonDto>{public Task<FinancingComparisonDto> Handle(CompareFinancingCommand c,CancellationToken ct){var sac=FinancingCalculator.Simulate(c.AssetValue,c.DownPayment,AmortizationSystem.SAC,c.AnnualInterestRate,c.MonthlyInsurance,c.MonthlyFees,c.TermMonths,c.FirstDueDate); var price=FinancingCalculator.Simulate(c.AssetValue,c.DownPayment,AmortizationSystem.Price,c.AnnualInterestRate,c.MonthlyInsurance,c.MonthlyFees,c.TermMonths,c.FirstDueDate); var interestSavings=price.TotalInterest-sac.TotalInterest; var totalSavings=price.TotalPayment-sac.TotalPayment; var interestPct=price.TotalInterest==0?0:Math.Round((interestSavings/price.TotalInterest)*100,2); var totalPct=price.TotalPayment==0?0:Math.Round((totalSavings/price.TotalPayment)*100,2); return Task.FromResult(new FinancingComparisonDto(sac,price,interestSavings,interestPct,totalSavings,totalPct));}}
public class CreateFinancingHandler(IFinancingRepository repo):IRequestHandler<CreateFinancingCommand,FinancingDto>{public async Task<FinancingDto> Handle(CreateFinancingCommand c,CancellationToken ct){var financed=c.AssetValue-c.DownPayment; var f=new Financing{UserId=c.UserId,Name=c.Name,Type=c.Type,AmortizationSystem=c.AmortizationSystem,Institution=c.Institution,AssetValue=c.AssetValue,DownPayment=c.DownPayment,FinancedAmount=financed,AnnualInterestRate=c.AnnualInterestRate,MonthlyInsurance=c.MonthlyInsurance,MonthlyFees=c.MonthlyFees,CetAnnualRate=c.CetAnnualRate,TermMonths=c.TermMonths,FirstDueDate=c.FirstDueDate,LinkedAccountId=c.LinkedAccountId,Notes=c.Notes,PropertyAddress=c.PropertyAddress,PropertyRegistration=c.PropertyRegistration,VehicleBrand=c.VehicleBrand,VehicleModel=c.VehicleModel,VehicleYear=c.VehicleYear,VehiclePlate=c.VehiclePlate,Installments=FinancingCalculator.BuildSchedule(financed,c.AmortizationSystem,c.AnnualInterestRate,c.MonthlyInsurance,c.MonthlyFees,c.TermMonths,c.FirstDueDate)}; await repo.AddAsync(f); return f.ToDto();}}
public class DeleteFinancingHandler(IFinancingRepository repo):IRequestHandler<DeleteFinancingCommand>{public async Task Handle(DeleteFinancingCommand c,CancellationToken ct)=>await repo.DeleteAsync(c.Id,c.UserId);}
public class MarkFinancingInstallmentAsPaidHandler(IFinancingRepository repo):IRequestHandler<MarkFinancingInstallmentAsPaidCommand,FinancingDto>{public async Task<FinancingDto> Handle(MarkFinancingInstallmentAsPaidCommand c,CancellationToken ct){var f=await repo.GetByIdAsync(c.FinancingId,c.UserId)??throw new NotFoundException("Financiamento nao encontrado"); var installment=f.Installments.FirstOrDefault(x=>x.Number==c.Number)??throw new NotFoundException("Parcela nao encontrada"); installment.Status=FinancingInstallmentStatus.Paid; installment.PaidAt=c.PaidAt??DateTime.UtcNow; installment.PaidAmount=c.PaidAmount??installment.TotalPayment; if(c.LinkedTransactionId!=null) installment.LinkedTransactionId=c.LinkedTransactionId; if(f.Installments.All(x=>x.Status==FinancingInstallmentStatus.Paid)) f.Status=FinancingStatus.PaidOff; await repo.UpdateAsync(f); return f.ToDto();}}
public class SimulateExtraAmortizationHandler(IFinancingRepository repo):IRequestHandler<SimulateExtraAmortizationCommand,ExtraAmortizationSimulationDto>{public async Task<ExtraAmortizationSimulationDto> Handle(SimulateExtraAmortizationCommand c,CancellationToken ct){if(c.ExtraAmount<=0) throw new ValidationException("Valor de amortizacao deve ser positivo"); var f=await repo.GetByIdAsync(c.FinancingId,c.UserId)??throw new NotFoundException("Financiamento nao encontrado"); var paid=f.Installments.Count(i=>i.Status==FinancingInstallmentStatus.Paid); var remaining=f.Installments.Skip(paid).ToList(); if(remaining.Count==0) throw new ValidationException("Financiamento ja quitado"); var balance=Math.Max(0,remaining.First().OpeningBalance-c.ExtraAmount); var firstDue=remaining.First().DueDate; var originalInterest=remaining.Sum(i=>i.Interest); var schedule=FinancingCalculator.BuildSchedule(balance,f.AmortizationSystem,f.AnnualInterestRate,f.MonthlyInsurance,f.MonthlyFees,remaining.Count,firstDue); var trimmed=new List<FinancingInstallment>(); foreach(var row in schedule){trimmed.Add(row); if(row.ClosingBalance<=0) break;} var newInterest=trimmed.Sum(i=>i.Interest); var savings=originalInterest-newInterest; var savingsPct=originalInterest==0?0:Math.Round((savings/originalInterest)*100,2); return new ExtraAmortizationSimulationDto(c.ExtraAmount,originalInterest,newInterest,savings,savingsPct,remaining.Count,trimmed.Count,remaining.Count-trimmed.Count,trimmed.Last().TotalPayment,trimmed.Select(i=>i.ToDto()).ToList());}}
public class UpdateFinancingHandler(IFinancingRepository repo):IRequestHandler<UpdateFinancingCommand,FinancingDto>{public async Task<FinancingDto> Handle(UpdateFinancingCommand c,CancellationToken ct){
 var f=await repo.GetByIdAsync(c.Id,c.UserId)??throw new NotFoundException("Financiamento nao encontrado");
 var paid=f.Installments.Where(i=>i.Status==FinancingInstallmentStatus.Paid).ToList();
 f.Name=c.Name; f.Type=c.Type; f.AmortizationSystem=c.AmortizationSystem; f.Institution=c.Institution;
 f.AssetValue=c.AssetValue; f.DownPayment=c.DownPayment; f.FinancedAmount=c.AssetValue-c.DownPayment;
 f.AnnualInterestRate=c.AnnualInterestRate; f.MonthlyInsurance=c.MonthlyInsurance; f.MonthlyFees=c.MonthlyFees;
 f.CetAnnualRate=c.CetAnnualRate; f.TermMonths=c.TermMonths; f.FirstDueDate=c.FirstDueDate;
 f.LinkedAccountId=c.LinkedAccountId; f.Notes=c.Notes; f.PropertyAddress=c.PropertyAddress;
 f.PropertyRegistration=c.PropertyRegistration; f.VehicleBrand=c.VehicleBrand; f.VehicleModel=c.VehicleModel;
 f.VehicleYear=c.VehicleYear; f.VehiclePlate=c.VehiclePlate; f.UpdatedAt=DateTime.UtcNow;
 var newSchedule=FinancingCalculator.BuildSchedule(f.FinancedAmount,f.AmortizationSystem,f.AnnualInterestRate,f.MonthlyInsurance,f.MonthlyFees,f.TermMonths,f.FirstDueDate);
 foreach(var inst in newSchedule){var p=paid.FirstOrDefault(x=>x.Number==inst.Number); if(p!=null){inst.Status=FinancingInstallmentStatus.Paid; inst.PaidAt=p.PaidAt; inst.PaidAmount=p.PaidAmount; inst.LinkedTransactionId=p.LinkedTransactionId;}}
 f.Installments=newSchedule;
 if(f.Installments.Count>0&&f.Installments.All(x=>x.Status==FinancingInstallmentStatus.Paid)) f.Status=FinancingStatus.PaidOff;
 await repo.UpdateAsync(f); return f.ToDto();}}
public class LinkTransactionHandler(IFinancingRepository repo):IRequestHandler<LinkTransactionCommand,FinancingDto>{public async Task<FinancingDto> Handle(LinkTransactionCommand c,CancellationToken ct){
 var f=await repo.GetByIdAsync(c.FinancingId,c.UserId)??throw new NotFoundException("Financiamento nao encontrado");
 var inst=f.Installments.FirstOrDefault(x=>x.Number==c.Number)??throw new NotFoundException("Parcela nao encontrada");
 inst.LinkedTransactionId=c.TransactionId;
 await repo.UpdateAsync(f); return f.ToDto();}}
public class GetFinancingSummaryHandler(IFinancingRepository repo):IRequestHandler<GetFinancingSummaryQuery,FinancingSummaryDto>{public async Task<FinancingSummaryDto> Handle(GetFinancingSummaryQuery q,CancellationToken ct){
 var financings=(await repo.GetByUserAsync(q.UserId)).Where(f=>f.Status==FinancingStatus.Active).ToList();
 var totalRemaining=financings.Sum(f=>f.Installments.LastOrDefault(i=>i.Status==FinancingInstallmentStatus.Paid)?.ClosingBalance??f.FinancedAmount);
 var totalMonthly=financings.Sum(f=>f.Installments.FirstOrDefault(i=>i.Status!=FinancingInstallmentStatus.Paid)?.TotalPayment??0);
 var totalInterestRemaining=financings.Sum(f=>f.Installments.Where(i=>i.Status!=FinancingInstallmentStatus.Paid).Sum(i=>i.Interest));
 var totalFinanced=financings.Sum(f=>f.FinancedAmount);
 var totalPaid=financings.Sum(f=>f.Installments.Where(i=>i.Status==FinancingInstallmentStatus.Paid).Sum(i=>i.Amortization));
 var progress=totalFinanced==0?0:Math.Round(totalPaid/totalFinanced*100,2);
 var upcoming=financings.Select(f=>f.Installments.FirstOrDefault(i=>i.Status!=FinancingInstallmentStatus.Paid) is{} inst?new UpcomingInstallmentDto(f.Id,f.Name,inst.Number,inst.DueDate,inst.TotalPayment,inst.Status):null).Where(x=>x!=null).Cast<UpcomingInstallmentDto>().OrderBy(x=>x.DueDate).Take(5).ToList();
 return new FinancingSummaryDto(financings.Count,totalRemaining,totalMonthly,totalInterestRemaining,progress,upcoming);}}
