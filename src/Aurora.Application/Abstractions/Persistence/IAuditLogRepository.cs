using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IAuditLogRepository
{
    Task AddAsync(AuditEntry entry, CancellationToken ct = default);
    Task<List<AuditEntry>> GetByUserAsync(string userId, CancellationToken ct = default);
}
