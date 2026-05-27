using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Transactions.Create;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Aurora.Tests.Handlers;

public class CreateTransactionHandlerTests
{
    private readonly Mock<ITransactionRepository> _txRepo = new();
    private readonly Mock<IAccountRepository> _accRepo = new();
    private readonly Mock<ICategoryRepository> _catRepo = new();
    private readonly Mock<ICreditCardInvoiceRepository> _invoiceRepo = new();
    private readonly Mock<ICacheService> _cache = new();

    private CreateTransactionHandler CreateHandler() =>
        new(_txRepo.Object, _accRepo.Object, _catRepo.Object, _invoiceRepo.Object, _cache.Object);

    private static Account CheckingAccount(string id = "acc1") => new()
    {
        Id = id, UserId = "user1", Name = "Conta Corrente",
        Type = AccountType.CheckingAccount, CurrentBalance = 1000m,
    };

    private static Category ExpenseCategory(string id = "cat1") => new()
    {
        Id = id, UserId = "user1", Name = "Alimentação", Type = CategoryType.Expense,
    };

    [Fact]
    public async Task Deve_atualizar_saldo_quando_status_Paid()
    {
        var account = CheckingAccount();
        _accRepo.Setup(r => r.GetByIdAsync("acc1", "user1")).ReturnsAsync(account);
        _catRepo.Setup(r => r.GetByIdAsync("cat1", "user1")).ReturnsAsync(ExpenseCategory());
        _txRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);
        _accRepo.Setup(r => r.UpdateAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
        _cache.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        var cmd = new CreateTransactionCommand("user1", "acc1", "cat1", "Almoço",
            50m, TransactionType.Expense, TransactionStatus.Paid, DateTime.UtcNow, null, null);

        await CreateHandler().Handle(cmd, default);

        _accRepo.Verify(r => r.UpdateAsync(It.Is<Account>(a => a.CurrentBalance == 950m)), Times.Once);
    }

    [Fact]
    public async Task Nao_deve_atualizar_saldo_quando_status_Pending()
    {
        var account = CheckingAccount();
        _accRepo.Setup(r => r.GetByIdAsync("acc1", "user1")).ReturnsAsync(account);
        _catRepo.Setup(r => r.GetByIdAsync("cat1", "user1")).ReturnsAsync(ExpenseCategory());
        _txRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);
        _cache.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        var cmd = new CreateTransactionCommand("user1", "acc1", "cat1", "Almoço",
            50m, TransactionType.Expense, TransactionStatus.Pending, DateTime.UtcNow, null, null);

        await CreateHandler().Handle(cmd, default);

        _accRepo.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Never);
    }

    [Fact]
    public async Task Deve_lancar_excecao_se_conta_nao_existir()
    {
        _accRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((Account?)null);

        var cmd = new CreateTransactionCommand("user1", "acc-inexistente", "cat1", "Almoço",
            50m, TransactionType.Expense, TransactionStatus.Pending, DateTime.UtcNow, null, null);

        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*Conta*");
    }

    [Fact]
    public async Task Deve_lancar_excecao_se_categoria_nao_pertencer_ao_usuario()
    {
        _accRepo.Setup(r => r.GetByIdAsync("acc1", "user1")).ReturnsAsync(CheckingAccount());
        _catRepo.Setup(r => r.GetByIdAsync("cat-outra", "user1")).ReturnsAsync((Category?)null);

        var cmd = new CreateTransactionCommand("user1", "acc1", "cat-outra", "Almoço",
            50m, TransactionType.Expense, TransactionStatus.Pending, DateTime.UtcNow, null, null);

        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*Categoria*");
    }

    [Fact]
    public async Task Deve_lancar_excecao_se_conta_nao_pertencer_ao_usuario()
    {
        // GetByIdAsync filters by both id AND userId; null means doesn't belong to user
        _accRepo.Setup(r => r.GetByIdAsync("acc-outro", "user1")).ReturnsAsync((Account?)null);

        var cmd = new CreateTransactionCommand("user1", "acc-outro", "cat1", "Almoço",
            50m, TransactionType.Expense, TransactionStatus.Pending, DateTime.UtcNow, null, null);

        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*Conta*");
    }
}
