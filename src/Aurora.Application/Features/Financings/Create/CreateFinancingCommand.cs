using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Financings.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Financings.Create;

public record CreateFinancingCommand(
    string UserId,
    string Name,
    FinancingType Type,
    AmortizationSystem AmortizationSystem,
    string Institution,
    decimal AssetValue,
    decimal DownPayment,
    decimal AnnualInterestRate,
    decimal MonthlyInsurance,
    decimal MonthlyFees,
    decimal? CetAnnualRate,
    int TermMonths,
    DateTime FirstDueDate,
    string? LinkedAccountId = null,
    string? Notes = null,
    string? PropertyAddress = null,
    string? PropertyRegistration = null,
    string? VehicleBrand = null,
    string? VehicleModel = null,
    int? VehicleYear = null,
    string? VehiclePlate = null) : IRequest<FinancingDto>;

public class CreateFinancingHandler(IFinancingRepository financings, ICacheService cache)
    : IRequestHandler<CreateFinancingCommand, FinancingDto>
{
    public async Task<FinancingDto> Handle(CreateFinancingCommand command, CancellationToken ct)
    {
        var financed = command.AssetValue - command.DownPayment;

        var f = new Financing
        {
            UserId = command.UserId,
            Name = command.Name,
            Type = command.Type,
            AmortizationSystem = command.AmortizationSystem,
            Institution = command.Institution,
            AssetValue = command.AssetValue,
            DownPayment = command.DownPayment,
            FinancedAmount = financed,
            AnnualInterestRate = command.AnnualInterestRate,
            MonthlyInsurance = command.MonthlyInsurance,
            MonthlyFees = command.MonthlyFees,
            CetAnnualRate = command.CetAnnualRate,
            TermMonths = command.TermMonths,
            FirstDueDate = command.FirstDueDate,
            LinkedAccountId = command.LinkedAccountId,
            Notes = command.Notes,
            PropertyAddress = command.PropertyAddress,
            PropertyRegistration = command.PropertyRegistration,
            VehicleBrand = command.VehicleBrand,
            VehicleModel = command.VehicleModel,
            VehicleYear = command.VehicleYear,
            VehiclePlate = command.VehiclePlate,
            Installments = FinancingCalculator.BuildSchedule(
                financed,
                command.AmortizationSystem,
                command.AnnualInterestRate,
                command.MonthlyInsurance,
                command.MonthlyFees,
                command.TermMonths,
                command.FirstDueDate)
        };

        await financings.AddAsync(f);
        await cache.RemoveByPrefixAsync(CacheKeys.DashboardPrefix(command.UserId), ct);
        return f.ToDto();
    }
}
