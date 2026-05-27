using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Transactions.Common;
using MediatR;

namespace Aurora.Application.Features.Dashboard.UpcomingDues;

public record UpcomingDuesQuery(string UserId, int Days = 7, string Status = "pending") : IRequest<List<UpcomingDueDto>>;

public class UpcomingDuesHandler(ITransactionRepository transactions) : IRequestHandler<UpcomingDuesQuery, List<UpcomingDueDto>>
{
    public async Task<List<UpcomingDueDto>> Handle(UpcomingDuesQuery query, CancellationToken ct) =>
        (await transactions.UpcomingDueAsync(query.UserId, query.Days <= 0 ? 7 : query.Days))
            .Select(x => x.ToUpcomingDueDto())
            .OrderBy(x => x.DueDate)
            .ToList();
}
