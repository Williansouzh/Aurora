using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.Transactions.Common;

public record TransactionDto(
    string Id,
    string AccountId,
    string CategoryId,
    string Description,
    decimal Amount,
    TransactionType Type,
    TransactionStatus Status,
    DateTime Date,
    DateTime? DueDate,
    string? Notes,
    string? CreditCardInvoiceId,
    bool IsRecurring,
    RecurrenceType? RecurrenceType,
    int RecurrenceInterval,
    DateTime? RecurrenceEndDate,
    Guid? RecurrenceGroupId,
    int? RecurrenceIndex,
    int? TotalInstallments);

public record UpcomingDueDto(
    string Id,
    string AccountId,
    string CategoryId,
    string Description,
    decimal Amount,
    TransactionType Type,
    TransactionStatus Status,
    DateTime Date,
    DateTime? DueDate,
    string? Notes,
    string? CreditCardInvoiceId,
    bool IsRecurring,
    RecurrenceType? RecurrenceType,
    int RecurrenceInterval,
    DateTime? RecurrenceEndDate,
    Guid? RecurrenceGroupId,
    int? RecurrenceIndex,
    int? TotalInstallments,
    int DaysUntilDue);

public static class TransactionMapper
{
    public static TransactionDto ToDto(this Transaction t) =>
        new(t.Id, t.AccountId, t.CategoryId, t.Description, t.Amount, t.Type, t.Status,
            t.Date, t.DueDate, t.Notes, t.CreditCardInvoiceId, t.IsRecurring,
            t.RecurrenceType, t.RecurrenceInterval, t.RecurrenceEndDate, t.RecurrenceGroupId,
            t.RecurrenceIndex, t.TotalInstallments);

    public static UpcomingDueDto ToUpcomingDueDto(this Transaction t)
    {
        var today = DateTime.UtcNow.Date;
        var due = t.DueDate?.Date ?? t.Date.Date;
        var days = (int)(due - today).TotalDays;
        return new UpcomingDueDto(
            t.Id, t.AccountId, t.CategoryId, t.Description, t.Amount, t.Type, t.Status,
            t.Date, t.DueDate, t.Notes, t.CreditCardInvoiceId, t.IsRecurring,
            t.RecurrenceType, t.RecurrenceInterval, t.RecurrenceEndDate, t.RecurrenceGroupId,
            t.RecurrenceIndex, t.TotalInstallments, days);
    }

    public static decimal Impact(TransactionType type, decimal amount) =>
        type == TransactionType.Income ? amount : -amount;
}
