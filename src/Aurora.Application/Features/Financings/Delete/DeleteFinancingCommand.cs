using Aurora.Application.Abstractions.Persistence;
using MediatR;

namespace Aurora.Application.Features.Financings.Delete;

public record DeleteFinancingCommand(string UserId, string Id) : IRequest;

public class DeleteFinancingHandler(IFinancingRepository financings) : IRequestHandler<DeleteFinancingCommand>
{
    public async Task Handle(DeleteFinancingCommand command, CancellationToken ct) =>
        await financings.DeleteAsync(command.Id, command.UserId);
}
