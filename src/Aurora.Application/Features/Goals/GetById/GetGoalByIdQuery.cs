using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Goals.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Goals.GetById;

public record GetGoalByIdQuery(string UserId, string Id) : IRequest<GoalDto>;

public class GetGoalByIdHandler(IGoalRepository repo) : IRequestHandler<GetGoalByIdQuery, GoalDto>
{
    public async Task<GoalDto> Handle(GetGoalByIdQuery q, CancellationToken ct)
    {
        var goal = await repo.GetByIdAsync(q.Id, q.UserId, ct)
            ?? throw new NotFoundException("Meta não encontrada.");
        return goal.ToDto();
    }
}
