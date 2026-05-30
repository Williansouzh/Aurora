using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Habits.Common;
using MediatR;

namespace Aurora.Application.Features.Habits.GetAll;

public record GetHabitsQuery(string UserId, bool ActiveOnly = true) : IRequest<List<HabitDto>>;

public class GetHabitsHandler(IHabitRepository repo)
    : IRequestHandler<GetHabitsQuery, List<HabitDto>>
{
    public async Task<List<HabitDto>> Handle(GetHabitsQuery query, CancellationToken ct)
    {
        var habits = query.ActiveOnly
            ? await repo.GetActiveAsync(query.UserId)
            : await repo.GetByUserAsync(query.UserId, ct);

        return habits.Select(h => h.ToDto()).ToList();
    }
}
