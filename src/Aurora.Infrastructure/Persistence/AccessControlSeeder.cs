using Aurora.Application.Common;
using Aurora.Application.Features.Auth.Common;
using Aurora.Application.Abstractions.Security;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence;

public static class AccessControlSeeder
{
    public static async Task SeedAsync(
        MongoContext ctx,
        IConfiguration configuration,
        IEncryptionService encryption,
        IPasswordHasher passwordHasher)
    {
        await SeedModulesAsync(ctx);
        await SeedLifeAreasAsync(ctx);
        var plans = await SeedPlansAsync(ctx);
        await EnsureDefaultSuperAdminAsync(ctx, configuration, plans, encryption, passwordHasher);
        await EnsureUserSubscriptionsAsync(ctx, plans);
        await PromoteConfiguredSuperAdminsAsync(ctx, configuration);
    }

    private static async Task SeedModulesAsync(MongoContext ctx)
    {
        var modules = new[]
        {
            Module(ModuleKeys.Home, "Home", "Aurora Home", "/", "layout-dashboard", 1, ModuleReleaseStage.Released),
            Module(ModuleKeys.Today, "Today", "Meu Dia", "/today", "calendar-check", 2, ModuleReleaseStage.Released),
            Module(ModuleKeys.Tasks, "Tasks", "Backlog", "/backlog", "inbox", 3, ModuleReleaseStage.Beta),
            Module(ModuleKeys.Habits, "Habits", "Rituais", "/habits", "flame", 4, ModuleReleaseStage.Beta),
            Module(ModuleKeys.Goals, "Goals", "Minha Jornada", "/goals", "target", 5, ModuleReleaseStage.Beta),
            Module(ModuleKeys.Timeline, "Timeline", "Linha da Vida", "/timeline", "scroll", 6, ModuleReleaseStage.Beta),
            Module(ModuleKeys.WeeklyPlanning, "WeeklyPlanning", "Minha Semana", "/weekly", "calendar-days", 7, ModuleReleaseStage.Beta),
            Module(ModuleKeys.Diary, "Diary", "Diario", "/diary", "book-open", 8, ModuleReleaseStage.Beta),
            Module(ModuleKeys.Evolution, "Evolution", "Evolucao", "/evolution", "camera", 9, ModuleReleaseStage.Beta),
            Module(ModuleKeys.Studies, "Studies", "Estudos", "/studies", "book-open-check", 10, ModuleReleaseStage.Beta),
            Module(ModuleKeys.Retrospectives, "Retrospectives", "Retrospectiva", "/retrospectives", "trending-up", 11, ModuleReleaseStage.Beta),
            Module(ModuleKeys.Finances, "Finances", "Dinheiro", "/transactions", "wallet", 20, ModuleReleaseStage.Released),
            Module(ModuleKeys.Admin, "Admin", "Super Admin", "/admin", "shield", 99, ModuleReleaseStage.Internal, UserRole.SuperAdmin),
        };

        foreach (var module in modules)
        {
            var existing = await ctx.ModuleCatalog.Find(x => x.Key == module.Key).FirstOrDefaultAsync();
            if (existing is null)
            {
                await ctx.ModuleCatalog.InsertOneAsync(module);
                continue;
            }

            existing.Name = module.Name;
            existing.ProductName = module.ProductName;
            existing.Route = module.Route;
            existing.Icon = module.Icon;
            existing.SortOrder = module.SortOrder;
            existing.RequiredRole = module.RequiredRole;
            existing.UpdatedAt = DateTime.UtcNow;
            await ctx.ModuleCatalog.ReplaceOneAsync(x => x.Id == existing.Id, existing);
        }
    }

    private static async Task SeedLifeAreasAsync(MongoContext ctx)
    {
        var areas = new[]
        {
            Area("health", LifeArea.Health, "Saude", "#10b981", "heart-pulse", 1),
            Area("work", LifeArea.Work, "Trabalho", "#2563eb", "briefcase", 2),
            Area("studies", LifeArea.Studies, "Estudos", "#7c3aed", "book-open", 3),
            Area("money", LifeArea.Money, "Dinheiro", "#16a34a", "wallet", 4),
            Area("relationships", LifeArea.Relationships, "Relacionamentos", "#db2777", "users", 5),
            Area("home", LifeArea.Home, "Casa", "#f97316", "home", 6),
            Area("leisure", LifeArea.Leisure, "Lazer", "#f59e0b", "sparkles", 7),
            Area("spirituality", LifeArea.Spirituality, "Espiritualidade", "#0f766e", "sun", 8),
            Area("projects", LifeArea.Projects, "Projetos", "#475569", "rocket", 9),
        };

        foreach (var area in areas)
        {
            var existing = await ctx.LifeAreaCatalog.Find(x => x.Key == area.Key).FirstOrDefaultAsync();
            if (existing is null)
            {
                await ctx.LifeAreaCatalog.InsertOneAsync(area);
            }
        }
    }

    private static async Task<Dictionary<string, Plan>> SeedPlansAsync(MongoContext ctx)
    {
        var plans = new[]
        {
            Plan("free", "Free", [ModuleKeys.Home, ModuleKeys.Today, ModuleKeys.Finances]),
            Plan("early-access", "Early Access", [
                ModuleKeys.Home,
                ModuleKeys.Today,
                ModuleKeys.Tasks,
                ModuleKeys.Habits,
                ModuleKeys.Goals,
                ModuleKeys.Timeline,
                ModuleKeys.WeeklyPlanning,
                ModuleKeys.Diary,
                ModuleKeys.Evolution,
                ModuleKeys.Studies,
                ModuleKeys.Retrospectives,
                ModuleKeys.Finances
            ]),
            Plan("pro", "Pro", [
                ModuleKeys.Home,
                ModuleKeys.Today,
                ModuleKeys.Tasks,
                ModuleKeys.Habits,
                ModuleKeys.Goals,
                ModuleKeys.Timeline,
                ModuleKeys.WeeklyPlanning,
                ModuleKeys.Diary,
                ModuleKeys.Evolution,
                ModuleKeys.Studies,
                ModuleKeys.Retrospectives,
                ModuleKeys.Finances
            ]),
            Plan("internal", "Internal", [
                ModuleKeys.Home,
                ModuleKeys.Today,
                ModuleKeys.Tasks,
                ModuleKeys.Habits,
                ModuleKeys.Goals,
                ModuleKeys.Timeline,
                ModuleKeys.WeeklyPlanning,
                ModuleKeys.Diary,
                ModuleKeys.Evolution,
                ModuleKeys.Studies,
                ModuleKeys.Retrospectives,
                ModuleKeys.Finances,
                ModuleKeys.Admin
            ]),
        };

        foreach (var plan in plans)
        {
            var existing = await ctx.Plans.Find(x => x.Key == plan.Key).FirstOrDefaultAsync();
            if (existing is null)
            {
                await ctx.Plans.InsertOneAsync(plan);
                continue;
            }

            existing.Name = plan.Name;
            existing.ModuleKeys = plan.ModuleKeys;
            existing.UpdatedAt = DateTime.UtcNow;
            await ctx.Plans.ReplaceOneAsync(x => x.Id == existing.Id, existing);
        }

        return await ctx.Plans
            .Find(_ => true)
            .ToListAsync()
            .ContinueWith(t => t.Result.ToDictionary(x => x.Key, x => x));
    }

    private static async Task EnsureUserSubscriptionsAsync(MongoContext ctx, Dictionary<string, Plan> plans)
    {
        var defaultPlan = plans["early-access"];
        var users = await ctx.Users.Find(x => x.DeletedAt == null).ToListAsync();

        foreach (var user in users)
        {
            var existing = await ctx.UserSubscriptions
                .Find(x => x.UserId == user.Id && x.Status != SubscriptionStatus.Cancelled && x.Status != SubscriptionStatus.Expired)
                .FirstOrDefaultAsync();

            if (existing is not null) continue;

            await ctx.UserSubscriptions.InsertOneAsync(new UserSubscription
            {
                Id = ObjectId.GenerateNewId().ToString(),
                UserId = user.Id,
                PlanId = defaultPlan.Id,
                Status = SubscriptionStatus.Active,
                StartedAt = DateTime.UtcNow
            });
        }
    }

    private static async Task PromoteConfiguredSuperAdminsAsync(MongoContext ctx, IConfiguration configuration)
    {
        var emails = configuration.GetSection("Admin:SuperAdminEmails").Get<string[]>() ?? [];
        if (emails.Length == 0) return;

        var internalPlan = await ctx.Plans.Find(x => x.Key == "internal").FirstOrDefaultAsync();
        foreach (var email in emails.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var user = await ctx.Users.Find(x => x.Email == email).FirstOrDefaultAsync();
            if (user is null) continue;

            user.Role = UserRole.SuperAdmin;
            user.Status = UserStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;
            await ctx.Users.ReplaceOneAsync(x => x.Id == user.Id, user);

            if (internalPlan is not null)
            {
                var existing = await ctx.UserSubscriptions
                    .Find(x => x.UserId == user.Id)
                    .SortByDescending(x => x.StartedAt)
                    .FirstOrDefaultAsync();

                var subscription = existing ?? new UserSubscription
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = user.Id,
                    StartedAt = DateTime.UtcNow
                };

                subscription.PlanId = internalPlan.Id;
                subscription.Status = SubscriptionStatus.Internal;
                subscription.UpdatedAt = DateTime.UtcNow;

                if (existing is null)
                {
                    await ctx.UserSubscriptions.InsertOneAsync(subscription);
                }
                else
                {
                    await ctx.UserSubscriptions.ReplaceOneAsync(
                        x => x.Id == subscription.Id,
                        subscription);
                }
            }
        }
    }

    private static async Task EnsureDefaultSuperAdminAsync(
        MongoContext ctx,
        IConfiguration configuration,
        Dictionary<string, Plan> plans,
        IEncryptionService encryption,
        IPasswordHasher passwordHasher)
    {
        var section = configuration.GetSection("Admin:DefaultSuperAdmin");
        var email = section["Email"]?.Trim();
        var password = section["Password"];
        var name = section["Name"]?.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            email = "superadmin@aurora.com.br";
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            password = "AuroraAdmin#2026!";
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Aurora Super Admin";
        }

        var normalizedEmail = UserSecurityMapper.NormalizeEmail(email);
        var emailHash = encryption.HashDeterministic(normalizedEmail);

        var user = await ctx.Users
            .Find(x => x.EmailHash == emailHash || x.Email == normalizedEmail)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            user = new User
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = name,
                PasswordHash = passwordHasher.Hash(password),
                IsEmailConfirmed = true,
                EmailConfirmedAt = DateTime.UtcNow,
                IsMfaEnabled = false,
                Role = UserRole.SuperAdmin,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            UserSecurityMapper.SetEmail(user, normalizedEmail, encryption);
            await ctx.Users.InsertOneAsync(user);
        }
        else
        {
            user.Name = string.IsNullOrWhiteSpace(user.Name) ? name : user.Name;
            user.Role = UserRole.SuperAdmin;
            user.Status = UserStatus.Active;
            user.IsEmailConfirmed = true;
            user.EmailConfirmedAt ??= DateTime.UtcNow;
            user.IsMfaEnabled = false;
            if (string.IsNullOrWhiteSpace(user.EmailHash) || string.IsNullOrWhiteSpace(user.EmailEncrypted))
            {
                UserSecurityMapper.SetEmail(user, normalizedEmail, encryption);
            }
            user.UpdatedAt = DateTime.UtcNow;
            await ctx.Users.ReplaceOneAsync(x => x.Id == user.Id, user);
        }

        if (plans.TryGetValue("internal", out var internalPlan))
        {
            await UpsertInternalSubscriptionAsync(ctx, user.Id, internalPlan.Id);
        }
    }

    private static async Task UpsertInternalSubscriptionAsync(MongoContext ctx, string userId, string planId)
    {
        var existing = await ctx.UserSubscriptions
            .Find(x => x.UserId == userId)
            .SortByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync();

        var subscription = existing ?? new UserSubscription
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            StartedAt = DateTime.UtcNow
        };

        subscription.PlanId = planId;
        subscription.Status = SubscriptionStatus.Internal;
        subscription.UpdatedAt = DateTime.UtcNow;

        if (existing is null)
        {
            await ctx.UserSubscriptions.InsertOneAsync(subscription);
        }
        else
        {
            await ctx.UserSubscriptions.ReplaceOneAsync(x => x.Id == subscription.Id, subscription);
        }
    }

    private static ModuleCatalogItem Module(
        string key,
        string name,
        string productName,
        string route,
        string icon,
        int sortOrder,
        ModuleReleaseStage stage,
        UserRole requiredRole = UserRole.User) => new()
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Key = key,
            Name = name,
            ProductName = productName,
            Route = route,
            Icon = icon,
            SortOrder = sortOrder,
            ReleaseStage = stage,
            RequiredRole = requiredRole,
            Status = ModuleStatus.Enabled,
            ShowInNavigation = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static Plan Plan(string key, string name, List<string> modules) => new()
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Key = key,
        Name = name,
        Status = PlanStatus.Active,
        ModuleKeys = modules,
        Limits = key switch
        {
            "free" => new Dictionary<string, int> { ["goals"] = 3, ["habits"] = 3, ["evolutionPhotos"] = 0 },
            "early-access" => new Dictionary<string, int> { ["goals"] = 20, ["habits"] = 20, ["evolutionPhotos"] = 100 },
            _ => new Dictionary<string, int> { ["goals"] = 100, ["habits"] = 100, ["evolutionPhotos"] = 1000 }
        },
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static LifeAreaCatalogItem Area(string key, LifeArea area, string name, string color, string icon, int sortOrder) => new()
    {
        Id = ObjectId.GenerateNewId().ToString(),
        Key = key,
        Area = area,
        Name = name,
        Color = color,
        Icon = icon,
        SortOrder = sortOrder,
        Status = ModuleStatus.Enabled,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
