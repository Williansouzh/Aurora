using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Budgets.Delete;

public record DeleteBudgetCommand(string UserId, string Id) : IRequest;

public class DeleteBudgetHandler(IBudgetRepository budgets, ICacheService cache) : IRequestHandler<DeleteBudgetCommand>
{
    public async Task Handle(DeleteBudgetCommand command, CancellationToken ct)
    {
        var budget = await budgets.GetByIdAsync(command.Id, command.UserId)
            ?? throw new NotFoundException("Orcamento nao encontrado");

        await budgets.DeleteAsync(budget.Id, command.UserId);
        await cache.RemoveByPrefixAsync(CacheKeys.DashboardPrefix(command.UserId), ct);
    }
}
