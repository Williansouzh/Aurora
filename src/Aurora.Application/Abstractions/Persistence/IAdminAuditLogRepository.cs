using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IAdminAuditLogRepository
{
    Task AddAsync(AdminAuditLog log, CancellationToken ct = default);
    Task<List<AdminAuditLog>> GetRecentAsync(int limit = 100, CancellationToken ct = default);
}
