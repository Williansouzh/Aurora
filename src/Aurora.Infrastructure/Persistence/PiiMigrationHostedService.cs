using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Infrastructure.Persistence.Mongo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence;

public class PiiMigrationHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<PiiMigrationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MongoContext>();
        var encryption = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

        var filter = Builders<Domain.Entities.User>.Filter.And(
            Builders<Domain.Entities.User>.Filter.Ne(x => x.Email, string.Empty),
            Builders<Domain.Entities.User>.Filter.Or(
                Builders<Domain.Entities.User>.Filter.Eq(x => x.EmailHash, string.Empty),
                Builders<Domain.Entities.User>.Filter.Eq(x => x.EmailEncrypted, string.Empty)));

        var users = await context.Users.Find(filter).ToListAsync(cancellationToken);
        foreach (var user in users)
        {
            UserSecurityMapper.SetEmail(user, user.Email, encryption);
            user.UpdatedAt = DateTime.UtcNow;
            await context.Users.ReplaceOneAsync(x => x.Id == user.Id, user, cancellationToken: cancellationToken);
        }

        if (users.Count > 0)
        {
            logger.LogInformation("Migrated PII fields for {Count} users", users.Count);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
