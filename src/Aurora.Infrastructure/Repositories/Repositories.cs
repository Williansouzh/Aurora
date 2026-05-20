using Aurora.Application.Interfaces;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Repositories;
public class UserRepository(MongoContext c):IUserRepository{public Task<User?> GetByEmailAsync(string email)=>c.Users.Find(x=>x.Email==email).FirstOrDefaultAsync(); public Task<User?> GetByIdAsync(string id)=>c.Users.Find(x=>x.Id==id).FirstOrDefaultAsync(); public Task AddAsync(User user)=>c.Users.InsertOneAsync(user);}
public class AccountRepository(MongoContext c):IAccountRepository{public Task<List<Account>> GetByUserAsync(string userId)=>c.Accounts.Find(x=>x.UserId==userId).ToListAsync(); public Task<Account?> GetByIdAsync(string id,string userId)=>c.Accounts.Find(x=>x.Id==id&&x.UserId==userId).FirstOrDefaultAsync(); public Task AddAsync(Account a)=>c.Accounts.InsertOneAsync(a); public Task UpdateAsync(Account a){a.UpdatedAt=DateTime.UtcNow; return c.Accounts.ReplaceOneAsync(x=>x.Id==a.Id&&x.UserId==a.UserId,a);} public Task DeleteAsync(string id,string userId)=>c.Accounts.DeleteOneAsync(x=>x.Id==id&&x.UserId==userId); public async Task<decimal> GetTotalBalanceAsync(string userId)=>(await c.Accounts.Find(x=>x.UserId==userId&&!x.IsArchived).ToListAsync()).Sum(x=>x.CurrentBalance);}
public class CategoryRepository(MongoContext c):ICategoryRepository{public Task<List<Category>> GetByUserAsync(string userId)=>c.Categories.Find(x=>x.UserId==userId).ToListAsync(); public Task<Category?> GetByIdAsync(string id,string userId)=>c.Categories.Find(x=>x.Id==id&&x.UserId==userId).FirstOrDefaultAsync(); public Task AddAsync(Category x)=>c.Categories.InsertOneAsync(x); public Task UpdateAsync(Category x){x.UpdatedAt=DateTime.UtcNow; return c.Categories.ReplaceOneAsync(a=>a.Id==x.Id&&a.UserId==x.UserId,x);} public Task DeleteAsync(string id,string userId)=>c.Categories.DeleteOneAsync(x=>x.Id==id&&x.UserId==userId);
public async Task SeedDefaultsAsync(string userId){ var i=new[]{"Salário","Freelance","Investimentos"}.Select(n=>new Category{UserId=userId,Name=n,Type=CategoryType.Income,IsDefault=true}); var e=new[]{"Moradia","Alimentação","Transporte","Saúde","Lazer","Educação","Assinaturas","Outros"}.Select(n=>new Category{UserId=userId,Name=n,Type=CategoryType.Expense,IsDefault=true}); await c.Categories.InsertManyAsync(i.Concat(e)); }}
public class TransactionRepository(MongoContext c):ITransactionRepository{
 public async Task<List<Transaction>> GetByFilterAsync(string userId,int? month,int? year,TransactionType? type,TransactionStatus? status,string? categoryId,string? accountId){ var f=Builders<Transaction>.Filter.Eq(x=>x.UserId,userId); if(month.HasValue&&year.HasValue){var s=new DateTime(year.Value,month.Value,1); var e=s.AddMonths(1); f &= Builders<Transaction>.Filter.Gte(x=>x.Date,s) & Builders<Transaction>.Filter.Lt(x=>x.Date,e);} if(type.HasValue)f &= Builders<Transaction>.Filter.Eq(x=>x.Type,type); if(status.HasValue)f &= Builders<Transaction>.Filter.Eq(x=>x.Status,status); if(!string.IsNullOrWhiteSpace(categoryId))f &= Builders<Transaction>.Filter.Eq(x=>x.CategoryId,categoryId); if(!string.IsNullOrWhiteSpace(accountId))f &= Builders<Transaction>.Filter.Eq(x=>x.AccountId,accountId); return await c.Transactions.Find(f).SortByDescending(x=>x.Date).ToListAsync(); }
 public Task<Transaction?> GetByIdAsync(string id,string userId)=>c.Transactions.Find(x=>x.Id==id&&x.UserId==userId).FirstOrDefaultAsync(); public Task AddAsync(Transaction tx)=>c.Transactions.InsertOneAsync(tx); public Task UpdateAsync(Transaction tx){tx.UpdatedAt=DateTime.UtcNow; return c.Transactions.ReplaceOneAsync(x=>x.Id==tx.Id&&x.UserId==tx.UserId,tx);} public Task DeleteAsync(string id,string userId)=>c.Transactions.DeleteOneAsync(x=>x.Id==id&&x.UserId==userId);
 public Task<bool> ExistsByAccountIdAsync(string accountId,string userId)=>c.Transactions.Find(x=>x.AccountId==accountId&&x.UserId==userId).AnyAsync(); public Task<bool> ExistsByCategoryIdAsync(string categoryId,string userId)=>c.Transactions.Find(x=>x.CategoryId==categoryId&&x.UserId==userId).AnyAsync();
 public async Task<decimal> SumAsync(string userId,int month,int year,TransactionType type,TransactionStatus status){
  var start=new DateTime(year,month,1); var end=start.AddMonths(1);
  var f=Builders<Transaction>.Filter.Eq(x=>x.UserId,userId) & Builders<Transaction>.Filter.Gte(x=>x.Date,start) & Builders<Transaction>.Filter.Lt(x=>x.Date,end) & Builders<Transaction>.Filter.Eq(x=>x.Type,type) & Builders<Transaction>.Filter.Eq(x=>x.Status,status);
  var r=await c.Transactions.Aggregate().Match(f).Group(_=>1,g=>new { Total=g.Sum(x=>x.Amount)}).FirstOrDefaultAsync();
  return r?.Total ?? 0m;
 }
 public async Task<int> CountAsync(string userId,int month,int year,TransactionStatus status){
  var start=new DateTime(year,month,1); var end=start.AddMonths(1);
  var f=Builders<Transaction>.Filter.Eq(x=>x.UserId,userId) & Builders<Transaction>.Filter.Gte(x=>x.Date,start) & Builders<Transaction>.Filter.Lt(x=>x.Date,end) & Builders<Transaction>.Filter.Eq(x=>x.Status,status);
  return (int)await c.Transactions.CountDocumentsAsync(f);
 }
 public Task<List<Transaction>> RecentAsync(string userId,int limit=5)=>c.Transactions.Find(x=>x.UserId==userId).SortByDescending(x=>x.Date).Limit(limit).ToListAsync();
 public async Task<List<(string CategoryId, decimal Total)>> CategoryExpenseAsync(string userId,int month,int year)=>(await GetByFilterAsync(userId,month,year,TransactionType.Expense,TransactionStatus.Paid,null,null)).GroupBy(x=>x.CategoryId).Select(g=>(g.Key,g.Sum(x=>x.Amount))).ToList();
 public async Task<List<(int Month, decimal Income, decimal Expense)>> CashFlowAsync(string userId,int year){var list=new List<(int,decimal,decimal)>(); for(var m=1;m<=12;m++){var i=await SumAsync(userId,m,year,TransactionType.Income,TransactionStatus.Paid); var e=await SumAsync(userId,m,year,TransactionType.Expense,TransactionStatus.Paid); list.Add((m,i,e));} return list; }
}
