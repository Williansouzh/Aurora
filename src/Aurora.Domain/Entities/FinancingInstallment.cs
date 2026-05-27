using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class FinancingInstallment
{
    public int Number { get; set; }
    public DateTime DueDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Amortization { get; set; }
    public decimal Interest { get; set; }
    public decimal Insurance { get; set; }
    public decimal Fees { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal ClosingBalance { get; set; }
    public FinancingInstallmentStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? LinkedTransactionId { get; set; }
}
