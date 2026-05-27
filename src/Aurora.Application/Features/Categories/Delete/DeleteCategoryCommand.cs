using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Categories.Delete;

public record DeleteCategoryCommand(string UserId, string Id) : IRequest;

public class DeleteCategoryHandler(
    ICategoryRepository categories,
    ITransactionRepository transactions) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand command, CancellationToken ct)
    {
        if (await transactions.ExistsByCategoryIdAsync(command.Id, command.UserId))
        {
            throw new ConflictException("Categoria possui transacoes vinculadas");
        }

        await categories.DeleteAsync(command.Id, command.UserId);
    }
}
