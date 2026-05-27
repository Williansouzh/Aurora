using Aurora.Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Mongo;

public class MongoContext
{
    public IMongoClient Client { get; }
    public IMongoDatabase Db { get; }

    public MongoContext(IOptions<MongoSettings> options)
    {
        Client = new MongoClient(options.Value.ConnectionString);
        Db = Client.GetDatabase(options.Value.DatabaseName);
    }

    public IMongoCollection<User> Users => Db.GetCollection<User>("users");
    public IMongoCollection<Account> Accounts => Db.GetCollection<Account>("accounts");
    public IMongoCollection<Category> Categories => Db.GetCollection<Category>("categories");
    public IMongoCollection<Transaction> Transactions => Db.GetCollection<Transaction>("transactions");
    public IMongoCollection<CreditCardInvoice> CreditCardInvoices => Db.GetCollection<CreditCardInvoice>("creditCardInvoices");
    public IMongoCollection<Budget> Budgets => Db.GetCollection<Budget>("budgets");
    public IMongoCollection<Transfer> Transfers => Db.GetCollection<Transfer>("transfers");
    public IMongoCollection<Financing> Financings => Db.GetCollection<Financing>("financings");
    public IMongoCollection<RefreshToken> RefreshTokens => Db.GetCollection<RefreshToken>("refreshTokens");
    public IMongoCollection<AuthChallenge> AuthChallenges => Db.GetCollection<AuthChallenge>("authChallenges");
    public IMongoCollection<AuditEntry> AuditEntries => Db.GetCollection<AuditEntry>("auditEntries");
}
