using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using MediatR;

namespace Aurora.Application.Features.Financings.Delete;

public record DeleteFinancingCommand(string UserId, string Id) : IRequest;

public class DeleteFinancingHandler(IFinancingRepository financings, ICacheService cache)
    : IRequestHandler<DeleteFinancingCommand>
{
    public async Task Handle(DeleteFinancingCommand command, CancellationToken ct)
    {
        await financings.DeleteAsync(command.Id, command.UserId);
        await cache.RemoveByPrefixAsync(CacheKeys.DashboardPrefix(command.UserId), ct);
    }
}
