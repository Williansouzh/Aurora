using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Messaging;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Domain.Entities;
using Aurora.Infrastructure.Cache;
using Aurora.Infrastructure.Messaging;
using Aurora.Infrastructure.Persistence.Mongo;
using Aurora.Infrastructure.Persistence;
using Aurora.Infrastructure.Persistence.Repositories;
using Aurora.Infrastructure.Persistence.UnitOfWork;
using Aurora.Infrastructure.RateLimiting;
using Aurora.Infrastructure.Security;
using Aurora.Infrastructure.Storage;
using Aurora.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using StackExchange.Redis;

namespace Aurora.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterBsonSerializers();

        services.Configure<MongoSettings>(configuration.GetSection("Mongo"));
        services.Configure<StorageSettings>(configuration.GetSection("Storage"));
        services.AddScoped<IStorageService, MinioStorageService>();
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<EncryptionSettings>(configuration.GetSection("Encryption"));
        services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
        services.Configure<AuthOptions>(configuration.GetSection("Auth"));

        services.AddSingleton<MongoContext>();
        services.AddScoped<MongoUnitOfWork>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<MongoUnitOfWork>());
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IFinancingRepository, FinancingRepository>();
        services.AddScoped<ICreditCardInvoiceRepository, CreditCardInvoiceRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuthChallengeRepository, AuthChallengeRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Life OS — Fase 2-3
        services.AddScoped<IDailyTaskRepository, DailyTaskRepository>();
        services.AddScoped<IHabitRepository, HabitRepository>();
        services.AddScoped<IHabitCheckInRepository, HabitCheckInRepository>();
        services.AddScoped<ITimelineEventRepository, TimelineEventRepository>();

        // Gamification
        services.AddScoped<IXpRepository, XpRepository>();
        services.AddScoped<IXpService, Aurora.Application.Services.XpService>();

        // Life OS — Fase 4-7
        services.AddScoped<IGoalRepository, GoalRepository>();
        services.AddScoped<IWeeklyPlanRepository, WeeklyPlanRepository>();
        services.AddScoped<IDiaryEntryRepository, DiaryEntryRepository>();
        services.AddScoped<IEvolutionAlbumRepository, EvolutionAlbumRepository>();
        services.AddScoped<IEvolutionPhotoRepository, EvolutionPhotoRepository>();

        services.AddSingleton<IRateLimiter, RedisRateLimiter>();
        services.AddHostedService<Aurora.Infrastructure.Notifications.HabitReminderService>();
        services.AddHostedService<Aurora.Infrastructure.Notifications.WeeklyPlanningReminderService>();

        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEncryptionService, AesGcmEncryptionService>();
        services.AddScoped<IMfaCodeGenerator, MfaCodeGenerator>();
        services.AddScoped<Aurora.Application.Abstractions.Messaging.IEmailSender, SmtpEmailSender>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddHostedService<PiiMigrationHostedService>();

        var redisConn = configuration.GetConnectionString("Redis") ?? "redis:6379";
        services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConn));

        return services;
    }

    private static void RegisterBsonSerializers()
    {
        try
        {
            BsonSerializer.RegisterSerializer(new EncryptedStringSerializer());
        }
        catch (Exception)
        {
            // Serializer may already be registered in test hosts.
        }
    }

    public static async Task EnsureIndexesAsync(this IServiceProvider sp)
    {
        var ctx = sp.GetRequiredService<MongoContext>();

        await ctx.Users.Indexes.CreateOneAsync(new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(x => x.Email),
            new CreateIndexOptions { Unique = true }));

        await ctx.Users.Indexes.CreateOneAsync(new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(x => x.EmailHash),
            new CreateIndexOptions { Sparse = true }));

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

        await ctx.AuthChallenges.Indexes.CreateManyAsync([
            new CreateIndexModel<AuthChallenge>(Builders<AuthChallenge>.IndexKeys.Ascending(x => x.UserId)),
            new CreateIndexModel<AuthChallenge>(Builders<AuthChallenge>.IndexKeys.Ascending(x => x.TokenHash)),
            new CreateIndexModel<AuthChallenge>(
                Builders<AuthChallenge>.IndexKeys.Ascending(x => x.ExpiresAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.Zero })
        ]);

        await ctx.AuditEntries.Indexes.CreateManyAsync([
            new CreateIndexModel<AuditEntry>(Builders<AuditEntry>.IndexKeys.Ascending(x => x.UserId)),
            new CreateIndexModel<AuditEntry>(Builders<AuditEntry>.IndexKeys.Ascending(x => x.OccurredAt))
        ]);

        // Life OS indexes
        await ctx.DailyTasks.Indexes.CreateManyAsync([
            new CreateIndexModel<DailyTask>(Builders<DailyTask>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Date)),
            new CreateIndexModel<DailyTask>(Builders<DailyTask>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Status))
        ]);

        await ctx.Habits.Indexes.CreateManyAsync([
            new CreateIndexModel<Habit>(Builders<Habit>.IndexKeys.Ascending(x => x.UserId)),
            new CreateIndexModel<Habit>(Builders<Habit>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.IsActive))
        ]);

        await ctx.HabitCheckIns.Indexes.CreateManyAsync([
            new CreateIndexModel<HabitCheckIn>(Builders<HabitCheckIn>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Date)),
            new CreateIndexModel<HabitCheckIn>(Builders<HabitCheckIn>.IndexKeys.Ascending(x => x.HabitId).Ascending(x => x.Date)),
            new CreateIndexModel<HabitCheckIn>(
                Builders<HabitCheckIn>.IndexKeys.Ascending(x => x.HabitId).Ascending(x => x.UserId).Ascending(x => x.Date),
                new CreateIndexOptions { Unique = true })
        ]);

        await ctx.TimelineEvents.Indexes.CreateManyAsync([
            new CreateIndexModel<TimelineEvent>(Builders<TimelineEvent>.IndexKeys.Ascending(x => x.UserId).Descending(x => x.OccurredAt)),
            new CreateIndexModel<TimelineEvent>(Builders<TimelineEvent>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Type)),
            new CreateIndexModel<TimelineEvent>(Builders<TimelineEvent>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.IsFavorite))
        ]);

        await ctx.Goals.Indexes.CreateManyAsync([
            new CreateIndexModel<Goal>(Builders<Goal>.IndexKeys.Ascending(x => x.UserId)),
            new CreateIndexModel<Goal>(Builders<Goal>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Status))
        ]);

        await ctx.WeeklyPlans.Indexes.CreateManyAsync([
            new CreateIndexModel<WeeklyPlan>(Builders<WeeklyPlan>.IndexKeys.Ascending(x => x.UserId).Descending(x => x.WeekStart)),
            new CreateIndexModel<WeeklyPlan>(
                Builders<WeeklyPlan>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.WeekStart),
                new CreateIndexOptions { Unique = true })
        ]);

        await ctx.DiaryEntries.Indexes.CreateManyAsync([
            new CreateIndexModel<DiaryEntry>(Builders<DiaryEntry>.IndexKeys.Ascending(x => x.UserId).Descending(x => x.Date)),
            new CreateIndexModel<DiaryEntry>(
                Builders<DiaryEntry>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Date),
                new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<DiaryEntry>(Builders<DiaryEntry>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Mood))
        ]);

        await ctx.EvolutionAlbums.Indexes.CreateOneAsync(
            new CreateIndexModel<EvolutionAlbum>(Builders<EvolutionAlbum>.IndexKeys.Ascending(x => x.UserId)));

        await ctx.EvolutionPhotos.Indexes.CreateManyAsync([
            new CreateIndexModel<EvolutionPhoto>(Builders<EvolutionPhoto>.IndexKeys.Ascending(x => x.UserId).Descending(x => x.Date)),
            new CreateIndexModel<EvolutionPhoto>(Builders<EvolutionPhoto>.IndexKeys.Ascending(x => x.AlbumId).Ascending(x => x.UserId))
        ]);

        await ctx.XpEntries.Indexes.CreateManyAsync([
            new CreateIndexModel<XpEntry>(Builders<XpEntry>.IndexKeys.Ascending(x => x.UserId).Descending(x => x.OccurredAt)),
            new CreateIndexModel<XpEntry>(Builders<XpEntry>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Source).Ascending(x => x.OccurredAt))
        ]);
    }
}
