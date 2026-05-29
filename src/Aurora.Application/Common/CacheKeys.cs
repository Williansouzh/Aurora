namespace Aurora.Application.Common;

public static class CacheKeys
{
    private const string Root = "aurora";

    public static string DashboardPrefix(string userId) => $"{Root}:dashboard:{userId}";

    public static string MonthlySummary(string userId, int month, int year) =>
        $"{DashboardPrefix(userId)}:monthly-summary:{year}:{month}:v4";

    public static string CategoryExpenses(string userId, int month, int year) =>
        $"{DashboardPrefix(userId)}:category-expenses:{year}:{month}:v1";

    public static string CashFlow(string userId, int year) =>
        $"{DashboardPrefix(userId)}:cash-flow:{year}:v1";

    public static string Budgets(string userId, int month, int year) =>
        $"{DashboardPrefix(userId)}:budgets:{year}:{month}:v1";

    public static string FinancingSummary(string userId) =>
        $"{DashboardPrefix(userId)}:financing-summary:v1";
}
