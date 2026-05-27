using Aurora.Application.Features.Transactions.Create;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.Transactions.Common;

public static class RecurrenceGenerator
{
    public static List<Transaction> Generate(CreateTransactionCommand command)
    {
        var groupId = Guid.NewGuid();
        var type = command.TotalInstallments.HasValue
            ? RecurrenceType.Monthly
            : command.RecurrenceType ?? RecurrenceType.Monthly;
        var interval = Math.Max(1, command.RecurrenceInterval);
        var total = command.TotalInstallments
            ?? CalculateOpenEndedCount(command.Date, command.RecurrenceEndDate, type, interval);

        var list = new List<Transaction>();
        for (var i = 0; i < total; i++)
        {
            var date = AddInterval(command.Date, type, interval, i);
            if (command.RecurrenceEndDate.HasValue && date.Date > command.RecurrenceEndDate.Value.Date) break;

            list.Add(new Transaction
            {
                UserId = command.UserId,
                AccountId = command.AccountId,
                CategoryId = command.CategoryId,
                Description = command.TotalInstallments.HasValue
                    ? $"{command.Description} ({i + 1}/{total})"
                    : command.Description,
                Amount = command.Amount,
                Type = command.Type,
                Status = i == 0 ? command.Status : TransactionStatus.Pending,
                Date = date,
                DueDate = command.DueDate.HasValue
                    ? AddInterval(command.DueDate.Value, type, interval, i)
                    : null,
                Notes = command.Notes,
                IsRecurring = true,
                RecurrenceType = type,
                RecurrenceInterval = interval,
                RecurrenceEndDate = command.RecurrenceEndDate,
                RecurrenceGroupId = groupId,
                RecurrenceIndex = i + 1,
                TotalInstallments = command.TotalInstallments
            });
        }
        return list;
    }

    public static DateTime AddInterval(DateTime date, RecurrenceType type, int interval, int index) => type switch
    {
        RecurrenceType.Weekly => date.AddDays(7 * interval * index),
        RecurrenceType.Yearly => date.AddYears(interval * index),
        _ => date.AddMonths(interval * index)
    };

    private static int CalculateOpenEndedCount(DateTime start, DateTime? end, RecurrenceType type, int interval)
    {
        var limit = end?.Date ?? start.AddMonths(12).Date;
        var count = 0;
        var current = start;
        while (current.Date <= limit && count < 60)
        {
            count++;
            current = AddInterval(start, type, interval, count);
        }
        return Math.Max(count, 1);
    }
}
