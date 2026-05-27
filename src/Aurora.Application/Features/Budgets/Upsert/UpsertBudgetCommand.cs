using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Budgets.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Budgets.Upsert;

public record UpsertBudgetCommand(
    string UserId,
    string CategoryId,
    int Month,
    int Year,
    decimal LimitAmount) : IRequest<BudgetDto>;

public class UpsertBudgetHandler(
    IBudgetRepository budgets,
    ICategoryRepository categories,
    ITransactionRepository transactions) : IRequestHandler<UpsertBudgetCommand, BudgetDto>
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

        return BudgetMapper.ToBudgetDto(category, budget, spent, command.Month, command.Year);
    }
}
