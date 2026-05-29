using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Dashboard.Common;
using Aurora.Application.Features.Transactions.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Dashboard.MonthlySummary;

public record GetMonthlySummaryQuery(string UserId, int Month, int Year) : IRequest<MonthlySummaryDto>;

public class GetMonthlySummaryHandler(
    IAccountRepository accounts,
    ITransactionRepository transactions,
    ICacheService cache) : IRequestHandler<GetMonthlySummaryQuery, MonthlySummaryDto>
{
    public async Task<MonthlySummaryDto> Handle(GetMonthlySummaryQuery query, CancellationToken ct)
    {
        var key = CacheKeys.MonthlySummary(query.UserId, query.Month, query.Year);
        var cached = await cache.GetAsync<MonthlySummaryDto>(key, ct);
        if (cached is not null) return cached;

        var total = await accounts.GetTotalBalanceAsync(query.UserId);

        var income = await transactions.SumAsync(query.UserId, query.Month, query.Year, TransactionType.Income, TransactionStatus.Paid);
        var expense = await transactions.SumAsync(query.UserId, query.Month, query.Year, TransactionType.Expense, TransactionStatus.Paid);

        var previousMonth = new DateTime(query.Year, query.Month, 1).AddMonths(-1);
        var prevIncome = await transactions.SumAsync(query.UserId, previousMonth.Month, previousMonth.Year, TransactionType.Income, TransactionStatus.Paid);
        var prevExpense = await transactions.SumAsync(query.UserId, previousMonth.Month, previousMonth.Year, TransactionType.Expense, TransactionStatus.Paid);

        var incomeVar = Variation(income, prevIncome);
        var expenseVar = Variation(expense, prevExpense);
        var savings = income == 0 ? 0 : Math.Round(((income - expense) / income) * 100, 2);

        var pendingIncome = await transactions.SumAsync(query.UserId, query.Month, query.Year, TransactionType.Income, TransactionStatus.Pending);
        var pendingExpense = await transactions.SumAsync(query.UserId, query.Month, query.Year, TransactionType.Expense, TransactionStatus.Pending);
        var paidCount = await transactions.CountAsync(query.UserId, query.Month, query.Year, TransactionStatus.Paid);
        var pendingCount = await transactions.CountAsync(query.UserId, query.Month, query.Year, TransactionStatus.Pending);

        var recent = (await transactions.RecentAsync(query.UserId)).Select(x => x.ToDto()).ToList();
        var upcoming = (await transactions.UpcomingDueAsync(query.UserId, 7)).Select(x => x.ToUpcomingDueDto()).ToList();

        var dto = new MonthlySummaryDto(
            total, income, expense, income - expense,
            pendingIncome, pendingExpense, paidCount, pendingCount,
            recent, prevIncome, prevExpense, incomeVar, expenseVar, savings, upcoming);

        await cache.SetAsync(key, dto, TimeSpan.FromMinutes(5), ct);
        return dto;
    }

    private static decimal Variation(decimal current, decimal previous) =>
        previous == 0
            ? current == 0 ? 0 : 100
            : Math.Round(((current - previous) / previous) * 100, 2);
}
