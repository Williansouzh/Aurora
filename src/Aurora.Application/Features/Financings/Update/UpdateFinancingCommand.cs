using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Financings.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Financings.Update;

public record UpdateFinancingCommand(
    string UserId,
    string Id,
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

public class UpdateFinancingHandler(IFinancingRepository financings, ICacheService cache)
    : IRequestHandler<UpdateFinancingCommand, FinancingDto>
{
    public async Task<FinancingDto> Handle(UpdateFinancingCommand command, CancellationToken ct)
    {
        var f = await financings.GetByIdAsync(command.Id, command.UserId)
            ?? throw new NotFoundException("Financiamento nao encontrado");

        var paid = f.Installments.Where(i => i.Status == FinancingInstallmentStatus.Paid).ToList();

        f.Name = command.Name;
        f.Type = command.Type;
        f.AmortizationSystem = command.AmortizationSystem;
        f.Institution = command.Institution;
        f.AssetValue = command.AssetValue;
        f.DownPayment = command.DownPayment;
        f.FinancedAmount = command.AssetValue - command.DownPayment;
        f.AnnualInterestRate = command.AnnualInterestRate;
        f.MonthlyInsurance = command.MonthlyInsurance;
        f.MonthlyFees = command.MonthlyFees;
        f.CetAnnualRate = command.CetAnnualRate;
        f.TermMonths = command.TermMonths;
        f.FirstDueDate = command.FirstDueDate;
        f.LinkedAccountId = command.LinkedAccountId;
        f.Notes = command.Notes;
        f.PropertyAddress = command.PropertyAddress;
        f.PropertyRegistration = command.PropertyRegistration;
        f.VehicleBrand = command.VehicleBrand;
        f.VehicleModel = command.VehicleModel;
        f.VehicleYear = command.VehicleYear;
        f.VehiclePlate = command.VehiclePlate;
        f.UpdatedAt = DateTime.UtcNow;

        var newSchedule = FinancingCalculator.BuildSchedule(
            f.FinancedAmount,
            f.AmortizationSystem,
            f.AnnualInterestRate,
            f.MonthlyInsurance,
            f.MonthlyFees,
            f.TermMonths,
            f.FirstDueDate);

        foreach (var inst in newSchedule)
        {
            var prev = paid.FirstOrDefault(x => x.Number == inst.Number);
            if (prev != null)
            {
                inst.Status = FinancingInstallmentStatus.Paid;
                inst.PaidAt = prev.PaidAt;
                inst.PaidAmount = prev.PaidAmount;
                inst.LinkedTransactionId = prev.LinkedTransactionId;
            }
        }

        f.Installments = newSchedule;

        if (f.Installments.Count > 0 && f.Installments.All(x => x.Status == FinancingInstallmentStatus.Paid))
        {
            f.Status = FinancingStatus.PaidOff;
        }

        await financings.UpdateAsync(f);
        await cache.RemoveByPrefixAsync(CacheKeys.DashboardPrefix(command.UserId), ct);
        return f.ToDto();
    }
}
