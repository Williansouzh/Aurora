using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Budgets.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Budgets.Upsert;

public record UpsertBudgetCommand(
    string UserId,
    string CategoryId,
    int Month,
    int Year,
    decimal LimitAmount) : IRequest<BudgetDto>;

public class UpsertBudgetValidator : AbstractValidator<UpsertBudgetCommand>
{
    public UpsertBudgetValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).GreaterThanOrEqualTo(2000);
        RuleFor(x => x.LimitAmount).GreaterThan(0);
    }
}

public class UpsertBudgetHandler(
    IBudgetRepository budgets,
    ICategoryRepository categories,
    ITransactionRepository transactions,
    ITimelineEventRepository timelineRepo,
    ICacheService cache) : IRequestHandler<UpsertBudgetCommand, BudgetDto>
{
    public async Task<BudgetDto> Handle(UpsertBudgetCommand command, CancellationToken ct)
    {
        if (command.Month < 1 || command.Month > 12) throw new ValidationException("Mes invalido");
        if (command.Year < 2000) throw new ValidationException("Ano invalido");
        if (command.LimitAmount <= 0) throw new ValidationException("Limite deve ser positivo");

        var category = await categories.GetByIdAsync(command.CategoryId, command.UserId)
            ?? throw new ValidationException("Categoria invalida");

        if (category.Type != CategoryType.Expense)
        {
            throw new ValidationException("Orcamento deve usar categoria de despesa");
        }

        var budget = await budgets.GetByCategoryPeriodAsync(
            command.UserId, command.CategoryId, command.Month, command.Year);

        if (budget is null)
        {
            budget = new Budget
            {
                UserId = command.UserId,
                CategoryId = command.CategoryId,
                Month = command.Month,
                Year = command.Year,
                LimitAmount = command.LimitAmount
            };
            await budgets.AddAsync(budget);
        }
        else
        {
            budget.LimitAmount = command.LimitAmount;
            await budgets.UpdateAsync(budget);
        }

        var spent = (await transactions.CategoryExpenseAsync(command.UserId, command.Month, command.Year))
            .FirstOrDefault(x => x.CategoryId == command.CategoryId).Total;

        if (spent > command.LimitAmount)
        {
            var monthName = new DateTime(command.Year, command.Month, 1).ToString("MMMM/yyyy");
            await timelineRepo.AddFromModuleAsync(new TimelineEvent
            {
                UserId = command.UserId,
                Type = TimelineEventType.MonthlyBudgetClosed,
                Area = LifeArea.Money,
                Title = $"Orçamento ultrapassado: {category.Name}",
                Description = $"Gasto R$ {spent:N2} de R$ {command.LimitAmount:N2} em {monthName}.",
                OccurredAt = DateTime.UtcNow,
                SourceModule = "Finances",
                SourceId = budget.Id,
                Visibility = TimelineVisibility.Private,
            });
        }

        await cache.RemoveByPrefixAsync(CacheKeys.DashboardPrefix(command.UserId), ct);

        return BudgetMapper.ToBudgetDto(category, budget, spent, command.Month, command.Year);
    }
}
