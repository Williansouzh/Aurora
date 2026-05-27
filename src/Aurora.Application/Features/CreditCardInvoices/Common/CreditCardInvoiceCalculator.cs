namespace Aurora.Application.Features.CreditCardInvoices.Common;

public static class CreditCardInvoiceCalculator
{
    public static (int Month, int Year) Period(DateTime date, int closingDay)
    {
        var period = new DateTime(date.Year, date.Month, 1);
        if (date.Day > closingDay) period = period.AddMonths(1);
        return (period.Month, period.Year);
    }

    public static DateTime DueDate(int month, int year, int dueDay) =>
        new(year, month, Math.Clamp(dueDay, 1, 28));
}
