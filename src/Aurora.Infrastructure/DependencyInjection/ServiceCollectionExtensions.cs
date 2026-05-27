using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Cache;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence.Repositories;
using Aurora.Infrastructure.RateLimiting;
using Aurora.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StackExchange.Redis;

namespace Aurora.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoSettings>(configuration.GetSection("Mongo"));
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddSingleton<MongoContext>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IFinancingRepository, FinancingRepository>();
        services.AddScoped<ICreditCardInvoiceRepository, CreditCardInvoiceRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddSingleton<IRateLimiter, RedisRateLimiter>();

        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICacheService, RedisCacheService>();

        var redisConn = configuration.GetConnectionString("Redis") ?? "redis:6379";
        services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConn));

        return services;
    }

    public static async Task EnsureIndexesAsync(this IServiceProvider sp)
    {
        var ctx = sp.GetRequiredService<MongoContext>();

        await ctx.Users.Indexes.CreateOneAsync(new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(x => x.Email),
            new CreateIndexOptions { Unique = true }));

        await ctx.Accounts.Indexes.CreateManyAsync([
            new CreateIndexModel<Account>(Builders<Account>.IndexKeys.Ascending(x => x.UserId)),
            new CreateIndexModel<Account>(Builders<Account>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Name))
        ]);

        await ctx.Categories.Indexes.CreateManyAsync([
            new CreateIndexModel<Category>(Builders<Category>.IndexKeys.Ascending(x => x.UserId)),
            new CreateIndexModel<Category>(Builders<Category>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Type))
        ]);

        await ctx.Transactions.Indexes.CreateManyAsync([
            new CreateIndexModel<Transaction>(Builders<Transaction>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Date)),
            new CreateIndexModel<Transaction>(Builders<Transaction>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Status)),
            new CreateIndexModel<Transaction>(Builders<Transaction>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.AccountId)),
            new CreateIndexModel<Transaction>(Builders<Transaction>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.CategoryId)),
            new CreateIndexModel<Transaction>(Builders<Transaction>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Type)),
            new CreateIndexModel<Transaction>(Builders<Transaction>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.RecurrenceGroupId)),
            new CreateIndexModel<Transaction>(Builders<Transaction>.IndexKeys
                .Ascending(x => x.UserId).Ascending(x => x.Date).Ascending(x => x.Type).Ascending(x => x.Status))
        ]);

        await ctx.CreditCardInvoices.Indexes.CreateManyAsync([
            new CreateIndexModel<CreditCardInvoice>(Builders<CreditCardInvoice>.IndexKeys
                .Ascending(x => x.UserId).Ascending(x => x.AccountId)),
            new CreateIndexModel<CreditCardInvoice>(Builders<CreditCardInvoice>.IndexKeys
                .Ascending(x => x.UserId).Ascending(x => x.AccountId).Ascending(x => x.Month).Ascending(x => x.Year),
                new CreateIndexOptions { Unique = true })
        ]);

        await ctx.Budgets.Indexes.CreateManyAsync([
            new CreateIndexModel<Budget>(Builders<Budget>.IndexKeys
                .Ascending(x => x.UserId).Ascending(x => x.Month).Ascending(x => x.Year)),
            new CreateIndexModel<Budget>(Builders<Budget>.IndexKeys
                .Ascending(x => x.UserId).Ascending(x => x.CategoryId).Ascending(x => x.Month).Ascending(x => x.Year),
                new CreateIndexOptions { Unique = true })
        ]);

        await ctx.Transfers.Indexes.CreateManyAsync([
            new CreateIndexModel<Transfer>(Builders<Transfer>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Date)),
            new CreateIndexModel<Transfer>(Builders<Transfer>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.FromAccountId)),
            new CreateIndexModel<Transfer>(Builders<Transfer>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.ToAccountId))
        ]);

        await ctx.Financings.Indexes.CreateManyAsync([
            new CreateIndexModel<Financing>(Builders<Financing>.IndexKeys.Ascending(x => x.UserId)),
            new CreateIndexModel<Financing>(Builders<Financing>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Status))
        ]);

        await ctx.RefreshTokens.Indexes.CreateManyAsync([
            new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(x => x.TokenHash),
                new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<RefreshToken>(Builders<RefreshToken>.IndexKeys.Ascending(x => x.UserId)),
            new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(x => x.ExpiresAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.Zero })
        ]);
    }
}
