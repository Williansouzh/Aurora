using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class AuditLogRepository(MongoContext context) : IAuditLogRepository
{
    public Task AddAsync(AuditEntry entry, CancellationToken ct = default) =>
        context.AuditEntries.InsertOneAsync(entry, cancellationToken: ct);

    public Task<List<AuditEntry>> GetByUserAsync(string userId, CancellationToken ct = default) =>
        context.AuditEntries.Find(x => x.UserId == userId).ToListAsync(ct);
}
