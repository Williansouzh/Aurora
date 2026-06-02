using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class AdminAuditLogRepository(MongoContext context) : IAdminAuditLogRepository
{
    public Task AddAsync(AdminAuditLog log, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(log.Id)) log.Id = ObjectId.GenerateNewId().ToString();
        return context.AdminAuditLogs.InsertOneAsync(log, cancellationToken: ct);
    }

    public Task<List<AdminAuditLog>> GetRecentAsync(int limit = 100, CancellationToken ct = default) =>
        context.AdminAuditLogs
            .Find(_ => true)
            .SortByDescending(x => x.OccurredAt)
            .Limit(limit)
            .ToListAsync(ct);
}
