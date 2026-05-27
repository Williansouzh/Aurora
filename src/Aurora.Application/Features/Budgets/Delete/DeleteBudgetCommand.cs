using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Budgets.Delete;

public record DeleteBudgetCommand(string UserId, string Id) : IRequest;

public class DeleteBudgetHandler(IBudgetRepository budgets) : IRequestHandler<DeleteBudgetCommand>
{
    public async Task Handle(DeleteBudgetCommand command, CancellationToken ct)
    {
        var budget = await budgets.GetByIdAsync(command.Id, command.UserId)
            ?? throw new NotFoundException("Orcamento nao encontrado");

        await budgets.DeleteAsync(budget.Id, command.UserId);
    }
}
