using Aurora.Application.Features.Financings.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Financings.Simulate;

public record SimulateFinancingCommand(
    decimal AssetValue,
    decimal DownPayment,
    AmortizationSystem AmortizationSystem,
    decimal AnnualInterestRate,
    decimal MonthlyInsurance,
    decimal MonthlyFees,
    int TermMonths,
    DateTime FirstDueDate) : IRequest<FinancingSimulationDto>;

public class SimulateFinancingHandler : IRequestHandler<SimulateFinancingCommand, FinancingSimulationDto>
{
    public Task<FinancingSimulationDto> Handle(SimulateFinancingCommand command, CancellationToken ct) =>
        Task.FromResult(FinancingCalculator.Simulate(
            command.AssetValue,
            command.DownPayment,
            command.AmortizationSystem,
            command.AnnualInterestRate,
            command.MonthlyInsurance,
            command.MonthlyFees,
            command.TermMonths,
            command.FirstDueDate));
}
