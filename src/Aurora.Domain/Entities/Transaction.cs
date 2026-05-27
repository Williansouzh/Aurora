using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class Transaction : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public DateTime Date { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
    public string? CreditCardInvoiceId { get; set; }

    public bool IsRecurring { get; set; }
    public RecurrenceType? RecurrenceType { get; set; }
    public int RecurrenceInterval { get; set; } = 1;
    public DateTime? RecurrenceEndDate { get; set; }
    public Guid? RecurrenceGroupId { get; set; }
    public int? RecurrenceIndex { get; set; }
    public int? TotalInstallments { get; set; }
}
