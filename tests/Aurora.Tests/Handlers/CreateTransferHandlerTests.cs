using Xunit;
using Aurora.Application.Features;
using Aurora.Application.Interfaces;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace Aurora.Tests.Handlers;

public class CreateTransferHandlerTests
{
    private readonly Mock<ITransferRepository> _transferRepo = new();
    private readonly Mock<IAccountRepository> _accRepo = new();

    private CreateTransferHandler CreateHandler() =>
        new(_transferRepo.Object, _accRepo.Object);

    private static Account MakeAccount(string id, decimal balance = 1000m) => new()
    {
        Id = id, UserId = "user1", Name = id, Type = AccountType.CheckingAccount, CurrentBalance = balance,
    };

    [Fact]
    public async Task Deve_debitar_origem_e_creditar_destino()
    {
        var from = MakeAccount("from", 1000m);
        var to = MakeAccount("to", 200m);

        _accRepo.Setup(r => r.GetByIdAsync("from", "user1")).ReturnsAsync(from);
        _accRepo.Setup(r => r.GetByIdAsync("to", "user1")).ReturnsAsync(to);
        _accRepo.Setup(r => r.UpdateAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
        _transferRepo.Setup(r => r.AddAsync(It.IsAny<Transfer>())).Returns(Task.CompletedTask);

        await CreateHandler().Handle(
            new CreateTransferCommand("user1", "from", "to", 300m, DateTime.UtcNow, "Transferência"), default);

        from.CurrentBalance.Should().Be(700m);
        to.CurrentBalance.Should().Be(500m);
        _accRepo.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Deve_falhar_se_origem_igual_destino()
    {
        var act = () => CreateHandler().Handle(
            new CreateTransferCommand("user1", "acc1", "acc1", 100m, DateTime.UtcNow, "Teste"), default);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Conta de origem e destino devem ser diferentes*");
    }

    [Fact]
    public async Task Deve_reverter_origem_se_credito_falhar()
    {
        var from = MakeAccount("from", 1000m);
        var to = MakeAccount("to", 200m);

        _accRepo.Setup(r => r.GetByIdAsync("from", "user1")).ReturnsAsync(from);
        _accRepo.Setup(r => r.GetByIdAsync("to", "user1")).ReturnsAsync(to);

        var call = 0;
        _accRepo.Setup(r => r.UpdateAsync(It.IsAny<Account>())).Returns(() =>
        {
            call++;
            if (call == 2) throw new Exception("Falha simulada no crédito");
            return Task.CompletedTask;
        });

        var act = () => CreateHandler().Handle(
            new CreateTransferCommand("user1", "from", "to", 300m, DateTime.UtcNow, "Teste"), default);

        await act.Should().ThrowAsync<Exception>();
        from.CurrentBalance.Should().Be(1000m, "saldo da origem deve ser revertido após falha");
    }
}
