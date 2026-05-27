using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Aurora.Infrastructure.Persistence.Repositories;

public class CategoryRepository(MongoContext context, UnitOfWork.MongoUnitOfWork unitOfWork)
    : MongoRepositoryBase<Category>(context.Categories, unitOfWork), ICategoryRepository
{
    private static readonly string[] DefaultIncomeCategories =
        ["Salário", "Freelance", "Investimentos"];

    private static readonly string[] DefaultExpenseCategories =
        ["Moradia", "Alimentação", "Transporte", "Saúde", "Lazer", "Educação", "Assinaturas", "Outros"];

    public Task<List<Category>> GetByUserAsync(string userId) => base.GetByUserAsync(userId);

    public Task<Category?> GetByIdAsync(string id, string userId) => base.GetByIdAsync(id, userId);

    public Task AddAsync(Category category) => base.AddAsync(category);

    public Task UpdateAsync(Category category) => base.UpdateAsync(category);

    public Task DeleteAsync(string id, string userId) => base.DeleteAsync(id, userId);

    public Task SeedDefaultsAsync(string userId)
    {
        var income = DefaultIncomeCategories.Select(name => new Category
        {
            UserId = userId,
            Name = name,
            Type = CategoryType.Income,
            IsDefault = true
        });

        var expense = DefaultExpenseCategories.Select(name => new Category
        {
            UserId = userId,
            Name = name,
            Type = CategoryType.Expense,
            IsDefault = true
        });

        return Collection.InsertManyAsync(income.Concat(expense));
    }
}
