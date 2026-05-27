using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.Financings.Common;

public record FinancingInstallmentDto(
    int Number,
    DateTime DueDate,
    decimal OpeningBalance,
    decimal Amortization,
    decimal Interest,
    decimal Insurance,
    decimal Fees,
    decimal TotalPayment,
    decimal ClosingBalance,
    FinancingInstallmentStatus Status,
    DateTime? PaidAt = null,
    decimal? PaidAmount = null,
    string? LinkedTransactionId = null);

public record FinancingDto(
    string Id,
    string Name,
    FinancingType Type,
    AmortizationSystem AmortizationSystem,
    FinancingStatus Status,
    string Institution,
    decimal AssetValue,
    decimal DownPayment,
    decimal FinancedAmount,
    decimal AnnualInterestRate,
    decimal MonthlyInsurance,
    decimal MonthlyFees,
    decimal? CetAnnualRate,
    int TermMonths,
    DateTime FirstDueDate,
    decimal TotalPayment,
    decimal TotalInterest,
    decimal RemainingBalance,
    decimal PaidPrincipal,
    decimal PaidInterest,
    decimal PaidTotal,
    decimal ProgressPercentage,
    decimal InterestSharePercentage,
    int PaidInstallments,
    int RemainingInstallments,
    List<FinancingInstallmentDto> Installments,
    string? LinkedAccountId = null,
    string? Notes = null,
    string? PropertyAddress = null,
    string? PropertyRegistration = null,
    string? VehicleBrand = null,
    string? VehicleModel = null,
    int? VehicleYear = null,
    string? VehiclePlate = null);

public record FinancingSimulationDto(
    decimal FinancedAmount,
    decimal FirstPayment,
    decimal LastPayment,
    decimal TotalPayment,
    decimal TotalInterest,
    decimal AveragePayment,
    decimal InterestSharePercentage,
    List<FinancingInstallmentDto> Installments);

public record FinancingComparisonDto(
    FinancingSimulationDto Sac,
    FinancingSimulationDto Price,
    decimal SacInterestSavings,
    decimal SacInterestSavingsPercentage,
    decimal SacTotalSavings,
    decimal SacTotalSavingsPercentage);

public record ExtraAmortizationSimulationDto(
    decimal ExtraAmount,
    decimal OriginalRemainingInterest,
    decimal NewRemainingInterest,
    decimal InterestSavings,
    decimal InterestSavingsPercentage,
    int OriginalRemainingMonths,
    int NewRemainingMonths,
    int MonthsSaved,
    decimal NewLastPayment,
    List<FinancingInstallmentDto> Installments);

public record UpcomingInstallmentDto(
    string FinancingId,
    string FinancingName,
    int Number,
    DateTime DueDate,
    decimal TotalPayment,
    FinancingInstallmentStatus Status);

public record FinancingSummaryDto(
    int ActiveCount,
    decimal TotalRemainingBalance,
    decimal TotalMonthlyPayment,
    decimal TotalInterestRemaining,
    decimal OverallProgress,
    List<UpcomingInstallmentDto> UpcomingInstallments);

public static class FinancingMapper
{
    public static FinancingInstallmentDto ToDto(this FinancingInstallment x) =>
        new(x.Number, x.DueDate, x.OpeningBalance, x.Amortization, x.Interest, x.Insurance, x.Fees,
            x.TotalPayment, x.ClosingBalance, x.Status, x.PaidAt, x.PaidAmount, x.LinkedTransactionId);

    public static FinancingDto ToDto(this Financing x)
    {
        var total = x.Installments.Sum(i => i.TotalPayment);
        var totalInterest = x.Installments.Sum(i => i.Interest);
        var paid = x.Installments.Count(i => i.Status == FinancingInstallmentStatus.Paid);
        var remaining = x.Installments.Count - paid;
        var paidRows = x.Installments.Where(i => i.Status == FinancingInstallmentStatus.Paid).ToList();
        var paidPrincipal = paidRows.Sum(i => i.Amortization);
        var paidInterest = paidRows.Sum(i => i.Interest);
        var paidTotal = paidRows.Sum(i => i.TotalPayment);

        var remainingBalance = x.Installments.LastOrDefault(i => i.Status == FinancingInstallmentStatus.Paid)?.ClosingBalance
            ?? x.FinancedAmount;
        if (paid == x.Installments.Count && x.Installments.Count > 0) remainingBalance = 0;

        var progress = x.FinancedAmount == 0 ? 0 : Math.Round((paidPrincipal / x.FinancedAmount) * 100, 2);
        var interestShare = total == 0 ? 0 : Math.Round((totalInterest / total) * 100, 2);

        return new FinancingDto(
            x.Id, x.Name, x.Type, x.AmortizationSystem, x.Status, x.Institution,
            x.AssetValue, x.DownPayment, x.FinancedAmount, x.AnnualInterestRate,
            x.MonthlyInsurance, x.MonthlyFees, x.CetAnnualRate, x.TermMonths, x.FirstDueDate,
            total, totalInterest, remainingBalance, paidPrincipal, paidInterest, paidTotal,
            progress, interestShare, paid, remaining,
            x.Installments.Select(i => i.ToDto()).ToList(),
            x.LinkedAccountId, x.Notes, x.PropertyAddress, x.PropertyRegistration,
            x.VehicleBrand, x.VehicleModel, x.VehicleYear, x.VehiclePlate);
    }
}
