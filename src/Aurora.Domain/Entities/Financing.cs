using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class Financing : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public FinancingType Type { get; set; }
    public AmortizationSystem AmortizationSystem { get; set; }
    public FinancingStatus Status { get; set; } = FinancingStatus.Active;
    public string Institution { get; set; } = string.Empty;
    public decimal AssetValue { get; set; }
    public decimal DownPayment { get; set; }
    public decimal FinancedAmount { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public decimal MonthlyInsurance { get; set; }
    public decimal MonthlyFees { get; set; }
    public decimal? CetAnnualRate { get; set; }
    public int TermMonths { get; set; }
    public DateTime FirstDueDate { get; set; }
    public List<FinancingInstallment> Installments { get; set; } = [];
    public string? LinkedAccountId { get; set; }
    public string? Notes { get; set; }
    public string? PropertyAddress { get; set; }
    public string? PropertyRegistration { get; set; }
    public string? VehicleBrand { get; set; }
    public string? VehicleModel { get; set; }
    public int? VehicleYear { get; set; }
    public string? VehiclePlate { get; set; }
}
