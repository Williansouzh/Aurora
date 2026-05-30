using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Goals.SyncFinancial;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Dashboard.CloseMonth;

public record CloseMonthCommand(string UserId, int Month, int Year) : IRequest<CloseMonthResult>;

public record CloseMonthResult(decimal Income, decimal Expense, decimal Result, int XpEarned);

public class CloseMonthHandler(
    ITransactionRepository txRepo,
    IGoalRepository goalRepo,
    ITimelineEventRepository timelineRepo,
    IXpService xp)
    : IRequestHandler<CloseMonthCommand, CloseMonthResult>
{
    public async Task<CloseMonthResult> Handle(CloseMonthCommand cmd, CancellationToken ct)
    {
        var income = await txRepo.SumAsync(cmd.UserId, cmd.Month, cmd.Year, TransactionType.Income, TransactionStatus.Paid);
        var expense = await txRepo.SumAsync(cmd.UserId, cmd.Month, cmd.Year, TransactionType.Expense, TransactionStatus.Paid);
        var result = income - expense;
        const int xpEarned = 25;

        var monthName = new DateTime(cmd.Year, cmd.Month, 1).ToString("MMMM/yyyy");

        await timelineRepo.AddFromModuleAsync(new Domain.Entities.TimelineEvent
        {
            UserId = cmd.UserId,
            Type = TimelineEventType.MonthlyBudgetClosed,
            Area = LifeArea.Money,
            Title = $"Mês financeiro fechado: {monthName}",
            Description = $"Receitas: R$ {income:N2} | Despesas: R$ {expense:N2} | Resultado: R$ {result:N2}",
            OccurredAt = DateTime.UtcNow,
            SourceModule = "Finances",
            Visibility = TimelineVisibility.Private,
        });

        await xp.AwardAsync(cmd.UserId, XpSource.MonthlyClose, xpEarned, $"Fechamento do mês {monthName}", ct);

        // Sync active financial goals
        var goals = await goalRepo.GetByStatusAsync(cmd.UserId, GoalStatus.Active);
        foreach (var goal in goals.Where(g => g.Area == LifeArea.Money && g.LinkedCategoryId is not null))
        {
            goal.CurrentValue = expense;
            goal.UpdatedAt = DateTime.UtcNow;
            await goalRepo.UpdateAsync(goal, ct);
        }

        return new CloseMonthResult(income, expense, result, xpEarned);
    }
}
