namespace Aurora.Application.Abstractions.Common;

public interface IAuditService
{
    Task RecordAsync(
        string userId,
        string action,
        string entityType,
        string? entityId = null,
        string? metadata = null,
        CancellationToken ct = default);
}
