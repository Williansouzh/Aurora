using Aurora.Application.Features.Transactions.Common;

namespace Aurora.Application.Features.Dashboard.Common;

public record MonthlySummaryDto(
    decimal TotalBalance,
    decimal MonthlyIncome,
    decimal MonthlyExpense,
    decimal MonthlyResult,
    decimal PendingIncome,
    decimal PendingExpense,
    int PaidTransactionsCount,
    int PendingTransactionsCount,
    List<TransactionDto> RecentTransactions,
    decimal PreviousMonthIncome,
    decimal PreviousMonthExpense,
    decimal IncomeVariation,
    decimal ExpenseVariation,
    decimal SavingsRate,
    List<UpcomingDueDto> UpcomingDueTransactions);

public record CategoryExpenseDto(
    string CategoryId,
    string CategoryName,
    string CategoryColor,
    decimal Total,
    decimal Percentage);

public record CashFlowItemDto(int Month, decimal Income, decimal Expense, decimal Result);
