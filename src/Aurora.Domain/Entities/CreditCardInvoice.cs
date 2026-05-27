using Aurora.Domain.Common;
using Aurora.Domain.Enums;

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
}
