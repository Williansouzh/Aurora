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
    public IMongoCollection<Plan> Plans => Db.GetCollection<Plan>("plans");
    public IMongoCollection<ModuleCatalogItem> ModuleCatalog => Db.GetCollection<ModuleCatalogItem>("moduleCatalog");
    public IMongoCollection<UserSubscription> UserSubscriptions => Db.GetCollection<UserSubscription>("userSubscriptions");
    public IMongoCollection<UserModuleOverride> UserModuleOverrides => Db.GetCollection<UserModuleOverride>("userModuleOverrides");
    public IMongoCollection<LifeAreaCatalogItem> LifeAreaCatalog => Db.GetCollection<LifeAreaCatalogItem>("lifeAreaCatalog");
    public IMongoCollection<AdminAuditLog> AdminAuditLogs => Db.GetCollection<AdminAuditLog>("adminAuditLogs");

    // Life OS — Fase 2-3
    public IMongoCollection<DailyTask> DailyTasks => Db.GetCollection<DailyTask>("dailyTasks");
    public IMongoCollection<Habit> Habits => Db.GetCollection<Habit>("habits");
    public IMongoCollection<HabitCheckIn> HabitCheckIns => Db.GetCollection<HabitCheckIn>("habitCheckIns");
    public IMongoCollection<TimelineEvent> TimelineEvents => Db.GetCollection<TimelineEvent>("timelineEvents");

    // Life OS — Fase 4-7
    public IMongoCollection<Goal> Goals => Db.GetCollection<Goal>("goals");
    public IMongoCollection<WeeklyPlan> WeeklyPlans => Db.GetCollection<WeeklyPlan>("weeklyPlans");
    public IMongoCollection<DiaryEntry> DiaryEntries => Db.GetCollection<DiaryEntry>("diaryEntries");
    public IMongoCollection<EvolutionAlbum> EvolutionAlbums => Db.GetCollection<EvolutionAlbum>("evolutionAlbums");
    public IMongoCollection<EvolutionPhoto> EvolutionPhotos => Db.GetCollection<EvolutionPhoto>("evolutionPhotos");
    public IMongoCollection<StudySkill> StudySkills => Db.GetCollection<StudySkill>("studySkills");
    public IMongoCollection<StudyPriorityAssessment> StudyPriorityAssessments => Db.GetCollection<StudyPriorityAssessment>("studyPriorityAssessments");
    public IMongoCollection<StudySession> StudySessions => Db.GetCollection<StudySession>("studySessions");
    public IMongoCollection<StudyReview> StudyReviews => Db.GetCollection<StudyReview>("studyReviews");
    public IMongoCollection<StudyTopic> StudyTopics => Db.GetCollection<StudyTopic>("studyTopics");
    public IMongoCollection<StudyResource> StudyResources => Db.GetCollection<StudyResource>("studyResources");
    public IMongoCollection<StudyPracticeTask> StudyPracticeTasks => Db.GetCollection<StudyPracticeTask>("studyPracticeTasks");

    // Gamification
    public IMongoCollection<XpEntry> XpEntries => Db.GetCollection<XpEntry>("xpEntries");
}
