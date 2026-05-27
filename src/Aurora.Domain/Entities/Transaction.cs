using Aurora.Domain.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Events;
using Aurora.Domain.Exceptions;

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

    public void MarkAsPaid(DateTime occurredAt)
    {
        if (Status == TransactionStatus.Paid)
        {
            return;
        }

        EnsureValidAmount();
        Status = TransactionStatus.Paid;
        AddDomainEvent(new TransactionPaidEvent(Id, UserId, AccountId, Amount, occurredAt));
    }

    public void MarkAsPending(DateTime occurredAt)
    {
        if (Status == TransactionStatus.Pending)
        {
            return;
        }

        Status = TransactionStatus.Pending;
        AddDomainEvent(new TransactionPendingEvent(Id, UserId, AccountId, Amount, occurredAt));
    }

    public void ReplaceDetails(
        string accountId,
        string categoryId,
        string description,
        decimal amount,
        TransactionType type,
        TransactionStatus status,
        DateTime date,
        DateTime? dueDate,
        string? notes)
    {
        if (amount <= 0)
        {
            throw new ValidationException("Transaction amount must be positive");
        }

        AccountId = accountId;
        CategoryId = categoryId;
        Description = description;
        Amount = amount;
        Type = type;
        Status = status;
        Date = date;
        DueDate = dueDate;
        Notes = notes;
    }

    private void EnsureValidAmount()
    {
        if (Amount <= 0)
        {
            throw new ValidationException("Transaction amount must be positive");
        }
    }
}
