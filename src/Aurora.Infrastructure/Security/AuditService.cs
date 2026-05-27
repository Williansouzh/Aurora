using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Domain.Entities;

namespace Aurora.Infrastructure.Security;

public class AuditService(
    IAuditLogRepository auditLogs,
    IDateTimeProvider clock,
    IEncryptionService encryption) : IAuditService
{
    public Task RecordAsync(
        string userId,
        string action,
        string entityType,
        string? entityId = null,
        string? metadata = null,
        CancellationToken ct = default) =>
        auditLogs.AddAsync(new AuditEntry
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OccurredAt = clock.UtcNow,
            Metadata = metadata,
            IpHash = metadata is null ? null : encryption.HashDeterministic(metadata)
        }, ct);
}
