using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Today.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Today.GetToday;

public record GetTodayTasksQuery(string UserId, DateTime? Date) : IRequest<TodayResponse>;

public record TodayResponse(
    List<DailyTaskDto> Pending,
    List<DailyTaskDto> Completed,
    List<DailyTaskDto> Overdue);

public class GetTodayTasksHandler(IDailyTaskRepository repo)
    : IRequestHandler<GetTodayTasksQuery, TodayResponse>
{
    public async Task<TodayResponse> Handle(GetTodayTasksQuery query, CancellationToken ct)
    {
        var date = (query.Date ?? DateTime.UtcNow).Date;
        var today = await repo.GetByDateAsync(query.UserId, date);
        var overdue = await repo.GetOverdueAsync(query.UserId, date);

        return new TodayResponse(
            today.Where(t => t.Status == DailyTaskStatus.Pending).Select(t => t.ToDto()).ToList(),
            today.Where(t => t.Status == DailyTaskStatus.Completed).Select(t => t.ToDto()).ToList(),
            overdue.Select(t => t.ToDto()).ToList());
    }
}
