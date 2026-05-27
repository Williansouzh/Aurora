using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Financings.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Financings.SimulateExtraAmortization;

public record SimulateExtraAmortizationCommand(string UserId, string FinancingId, decimal ExtraAmount)
    : IRequest<ExtraAmortizationSimulationDto>;

public class SimulateExtraAmortizationHandler(IFinancingRepository financings)
    : IRequestHandler<SimulateExtraAmortizationCommand, ExtraAmortizationSimulationDto>
{
    public async Task<ExtraAmortizationSimulationDto> Handle(SimulateExtraAmortizationCommand command, CancellationToken ct)
    {
        if (command.ExtraAmount <= 0)
        {
            throw new ValidationException("Valor de amortizacao deve ser positivo");
        }

        var f = await financings.GetByIdAsync(command.FinancingId, command.UserId)
            ?? throw new NotFoundException("Financiamento nao encontrado");

        var paid = f.Installments.Count(i => i.Status == FinancingInstallmentStatus.Paid);
        var remaining = f.Installments.Skip(paid).ToList();

        if (remaining.Count == 0)
        {
            throw new ValidationException("Financiamento ja quitado");
        }

        var balance = Math.Max(0, remaining.First().OpeningBalance - command.ExtraAmount);
        var firstDue = remaining.First().DueDate;
        var originalInterest = remaining.Sum(i => i.Interest);

        var schedule = FinancingCalculator.BuildSchedule(
            balance,
            f.AmortizationSystem,
            f.AnnualInterestRate,
            f.MonthlyInsurance,
            f.MonthlyFees,
            remaining.Count,
            firstDue);

        var trimmed = new List<FinancingInstallment>();
        foreach (var row in schedule)
        {
            trimmed.Add(row);
            if (row.ClosingBalance <= 0) break;
        }

        var newInterest = trimmed.Sum(i => i.Interest);
        var savings = originalInterest - newInterest;
        var savingsPct = originalInterest == 0 ? 0 : Math.Round((savings / originalInterest) * 100, 2);

        return new ExtraAmortizationSimulationDto(
            command.ExtraAmount,
            originalInterest,
            newInterest,
            savings,
            savingsPct,
            remaining.Count,
            trimmed.Count,
            remaining.Count - trimmed.Count,
            trimmed.Last().TotalPayment,
            trimmed.Select(i => i.ToDto()).ToList());
    }
}
