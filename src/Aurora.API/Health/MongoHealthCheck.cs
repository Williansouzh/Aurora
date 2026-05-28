using Aurora.Infrastructure.Persistence.Mongo;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aurora.API.Health;

public class MongoHealthCheck(MongoContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthContext,
        CancellationToken cancellationToken = default)
    {
        await context.Db.RunCommandAsync((Command<BsonDocument>)"{ping:1}", cancellationToken: cancellationToken);
        return HealthCheckResult.Healthy();
    }
}
