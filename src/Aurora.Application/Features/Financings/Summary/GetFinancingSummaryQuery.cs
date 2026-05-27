using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Financings.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Financings.Summary;

public record GetFinancingSummaryQuery(string UserId) : IRequest<FinancingSummaryDto>;

public class GetFinancingSummaryHandler(IFinancingRepository financings)
    : IRequestHandler<GetFinancingSummaryQuery, FinancingSummaryDto>
{
    public async Task<FinancingSummaryDto> Handle(GetFinancingSummaryQuery query, CancellationToken ct)
    {
        var active = (await financings.GetByUserAsync(query.UserId))
            .Where(f => f.Status == FinancingStatus.Active)
            .ToList();

        var totalRemaining = active.Sum(f =>
            f.Installments.LastOrDefault(i => i.Status == FinancingInstallmentStatus.Paid)?.ClosingBalance
            ?? f.FinancedAmount);

        var totalMonthly = active.Sum(f =>
            f.Installments.FirstOrDefault(i => i.Status != FinancingInstallmentStatus.Paid)?.TotalPayment ?? 0);

        var totalInterestRemaining = active.Sum(f =>
            f.Installments.Where(i => i.Status != FinancingInstallmentStatus.Paid).Sum(i => i.Interest));

        var totalFinanced = active.Sum(f => f.FinancedAmount);
        var totalPaid = active.Sum(f =>
            f.Installments.Where(i => i.Status == FinancingInstallmentStatus.Paid).Sum(i => i.Amortization));

        var progress = totalFinanced == 0 ? 0 : Math.Round(totalPaid / totalFinanced * 100, 2);

        var upcoming = active
            .Select(f =>
            {
                var next = f.Installments.FirstOrDefault(i => i.Status != FinancingInstallmentStatus.Paid);
                return next is null
                    ? null
                    : new UpcomingInstallmentDto(f.Id, f.Name, next.Number, next.DueDate, next.TotalPayment, next.Status);
            })
            .Where(x => x != null)
            .Cast<UpcomingInstallmentDto>()
            .OrderBy(x => x.DueDate)
            .Take(5)
            .ToList();

        return new FinancingSummaryDto(active.Count, totalRemaining, totalMonthly, totalInterestRemaining, progress, upcoming);
    }
}
