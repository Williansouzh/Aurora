using Aurora.Domain.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;

namespace Aurora.Domain.Entities;

public class CreditCardInvoice : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal TotalAmount { get; set; }
    public CreditCardInvoiceStatus Status { get; set; } = CreditCardInvoiceStatus.Open;
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }

    public void AddCharge(decimal amount)
    {
        EnsurePositive(amount);
        TotalAmount += amount;
    }

    public void RemoveCharge(decimal amount)
    {
        EnsurePositive(amount);
        TotalAmount = Math.Max(0, TotalAmount - amount);
    }

    public void MarkAsPaid(DateTime paidAt)
    {
        if (Status == CreditCardInvoiceStatus.Paid)
        {
            throw new ValidationException("Invoice is already paid");
        }

        Status = CreditCardInvoiceStatus.Paid;
        PaidAt = paidAt;
    }

    private static void EnsurePositive(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ValidationException("Amount must be positive");
        }
    }
}
