using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Transactions.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Transactions.MarkAsPaid;

public record MarkTransactionAsPaidCommand(string UserId, string Id) : IRequest<TransactionDto>;

public class MarkTransactionAsPaidValidator : AbstractValidator<MarkTransactionAsPaidCommand>
{
    public MarkTransactionAsPaidValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class MarkTransactionAsPaidHandler(
    ITransactionRepository txRepo,
    IAccountRepository accRepo,
    IBudgetRepository budgetRepo,
    ICategoryRepository categoryRepo,
    ITimelineEventRepository timelineRepo,
    ICacheService cache) : IRequestHandler<MarkTransactionAsPaidCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(MarkTransactionAsPaidCommand command, CancellationToken ct)
    {
        var tx = await txRepo.GetByIdAsync(command.Id, command.UserId)
            ?? throw new NotFoundException("Transacao nao encontrada");

        if (tx.Status != TransactionStatus.Paid)
        {
            var account = await accRepo.GetByIdAsync(tx.AccountId, command.UserId)
                ?? throw new ValidationException("Conta invalida");

            if (account.Type != AccountType.CreditCard)
            {
                account.ApplyTransaction(tx.Type, tx.Amount);
                await accRepo.UpdateAsync(account);
            }

            tx.MarkAsPaid(DateTime.UtcNow);
            await txRepo.UpdateAsync(tx);

            if (tx.Type == TransactionType.Expense && !string.IsNullOrWhiteSpace(tx.CategoryId))
            {
                await CheckBudgetOverspentAsync(tx, command.UserId, ct);
            }
        }

        await cache.RemoveByPrefixAsync(CacheKeys.DashboardPrefix(command.UserId), ct);
        return tx.ToDto();
    }

    private async Task CheckBudgetOverspentAsync(Transaction tx, string userId, CancellationToken ct)
    {
        var budget = await budgetRepo.GetByCategoryPeriodAsync(
            userId, tx.CategoryId, tx.Date.Month, tx.Date.Year);

        if (budget is null) return;

        var byCategory = await txRepo.CategoryExpenseAsync(userId, tx.Date.Month, tx.Date.Year);
        var spent = byCategory.FirstOrDefault(x => x.CategoryId == tx.CategoryId).Total;

        if (spent <= budget.LimitAmount) return;

        var category = await categoryRepo.GetByIdAsync(tx.CategoryId, userId);
        var monthName = new DateTime(tx.Date.Year, tx.Date.Month, 1).ToString("MMMM/yyyy");

        await timelineRepo.AddFromModuleAsync(new TimelineEvent
        {
            UserId = userId,
            Type = TimelineEventType.MonthlyBudgetClosed,
            Area = LifeArea.Money,
            Title = $"Orçamento ultrapassado: {category?.Name ?? tx.CategoryId}",
            Description = $"Gasto R$ {spent:N2} de R$ {budget.LimitAmount:N2} em {monthName}.",
            OccurredAt = DateTime.UtcNow,
            SourceModule = "Finances",
            SourceId = budget.Id,
            Visibility = TimelineVisibility.Private,
        });
    }
}
