using Aurora.Application.Interfaces;
using Aurora.Infrastructure.Auth;
using Aurora.Infrastructure.Cache;
using Aurora.Infrastructure.Mongo;
using Aurora.Infrastructure.Repositories;
using Aurora.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StackExchange.Redis;

namespace Aurora.Infrastructure.Extensions;
public static class DependencyInjection {
 public static IServiceCollection AddInfrastructure(this IServiceCollection s, IConfiguration c){
  s.Configure<MongoSettings>(c.GetSection("Mongo")); s.Configure<JwtSettings>(c.GetSection("Jwt"));
  s.AddSingleton<MongoContext>(); s.AddScoped<IUserRepository,UserRepository>(); s.AddScoped<IAccountRepository,AccountRepository>(); s.AddScoped<ICategoryRepository,CategoryRepository>(); s.AddScoped<ITransactionRepository,TransactionRepository>();
  s.AddScoped<IPasswordHasher,PasswordHasher>(); s.AddScoped<IJwtTokenService,JwtTokenService>(); s.AddScoped<ICacheService,DistributedCacheService>();
  var redisConn=c.GetConnectionString("Redis")??"redis:6379";
  s.AddStackExchangeRedisCache(o=>o.Configuration=redisConn);
  s.AddSingleton<IConnectionMultiplexer>(_=>ConnectionMultiplexer.Connect(redisConn));
  return s;
 }
 public static async Task EnsureIndexesAsync(this IServiceProvider sp){ var ctx=sp.GetRequiredService<MongoContext>();
  await ctx.Users.Indexes.CreateOneAsync(new CreateIndexModel<Aurora.Domain.Entities.User>(Builders<Aurora.Domain.Entities.User>.IndexKeys.Ascending(x=>x.Email),new CreateIndexOptions{Unique=true}));
  await ctx.Accounts.Indexes.CreateManyAsync([
    new CreateIndexModel<Aurora.Domain.Entities.Account>(Builders<Aurora.Domain.Entities.Account>.IndexKeys.Ascending(x=>x.UserId)),
    new CreateIndexModel<Aurora.Domain.Entities.Account>(Builders<Aurora.Domain.Entities.Account>.IndexKeys.Ascending(x=>x.UserId).Ascending(x=>x.Name))
  ]);
  await ctx.Categories.Indexes.CreateManyAsync([
    new CreateIndexModel<Aurora.Domain.Entities.Category>(Builders<Aurora.Domain.Entities.Category>.IndexKeys.Ascending(x=>x.UserId)),
    new CreateIndexModel<Aurora.Domain.Entities.Category>(Builders<Aurora.Domain.Entities.Category>.IndexKeys.Ascending(x=>x.UserId).Ascending(x=>x.Type))
  ]);
  await ctx.Transactions.Indexes.CreateManyAsync([
    new CreateIndexModel<Aurora.Domain.Entities.Transaction>(Builders<Aurora.Domain.Entities.Transaction>.IndexKeys.Ascending(x=>x.UserId).Ascending(x=>x.Date)),
    new CreateIndexModel<Aurora.Domain.Entities.Transaction>(Builders<Aurora.Domain.Entities.Transaction>.IndexKeys.Ascending(x=>x.UserId).Ascending(x=>x.Status)),
    new CreateIndexModel<Aurora.Domain.Entities.Transaction>(Builders<Aurora.Domain.Entities.Transaction>.IndexKeys.Ascending(x=>x.UserId).Ascending(x=>x.AccountId)),
    new CreateIndexModel<Aurora.Domain.Entities.Transaction>(Builders<Aurora.Domain.Entities.Transaction>.IndexKeys.Ascending(x=>x.UserId).Ascending(x=>x.CategoryId)),
    new CreateIndexModel<Aurora.Domain.Entities.Transaction>(Builders<Aurora.Domain.Entities.Transaction>.IndexKeys.Ascending(x=>x.UserId).Ascending(x=>x.Type)),
    new CreateIndexModel<Aurora.Domain.Entities.Transaction>(Builders<Aurora.Domain.Entities.Transaction>.IndexKeys.Ascending(x=>x.UserId).Ascending(x=>x.Date).Ascending(x=>x.Type).Ascending(x=>x.Status))
  ]);
 }
}
