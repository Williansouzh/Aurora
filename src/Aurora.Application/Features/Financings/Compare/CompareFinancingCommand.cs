using Aurora.Application.Features.Financings.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Financings.Compare;

public record CompareFinancingCommand(
    decimal AssetValue,
    decimal DownPayment,
    decimal AnnualInterestRate,
    decimal MonthlyInsurance,
    decimal MonthlyFees,
    int TermMonths,
    DateTime FirstDueDate) : IRequest<FinancingComparisonDto>;

public class CompareFinancingHandler : IRequestHandler<CompareFinancingCommand, FinancingComparisonDto>
{
    public Task<FinancingComparisonDto> Handle(CompareFinancingCommand command, CancellationToken ct)
    {
        var sac = FinancingCalculator.Simulate(
            command.AssetValue, command.DownPayment, AmortizationSystem.SAC,
            command.AnnualInterestRate, command.MonthlyInsurance, command.MonthlyFees,
            command.TermMonths, command.FirstDueDate);

        var price = FinancingCalculator.Simulate(
            command.AssetValue, command.DownPayment, AmortizationSystem.Price,
            command.AnnualInterestRate, command.MonthlyInsurance, command.MonthlyFees,
            command.TermMonths, command.FirstDueDate);

        var interestSavings = price.TotalInterest - sac.TotalInterest;
        var totalSavings = price.TotalPayment - sac.TotalPayment;
        var interestPct = price.TotalInterest == 0 ? 0 : Math.Round((interestSavings / price.TotalInterest) * 100, 2);
        var totalPct = price.TotalPayment == 0 ? 0 : Math.Round((totalSavings / price.TotalPayment) * 100, 2);

        return Task.FromResult(new FinancingComparisonDto(sac, price, interestSavings, interestPct, totalSavings, totalPct));
    }
}
