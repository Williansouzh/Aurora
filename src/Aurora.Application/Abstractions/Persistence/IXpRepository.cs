using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Abstractions.Persistence;

public interface IXpRepository
{
    Task AddAsync(XpEntry entry, CancellationToken ct = default);
    Task<List<XpEntry>> GetRecentAsync(string userId, int limit = 20);
    Task<int> GetTotalForPeriodAsync(string userId, DateTime from, DateTime to);
    Task<bool> ExistsAsync(string userId, XpSource source, DateTime periodStart, DateTime periodEnd);
}
