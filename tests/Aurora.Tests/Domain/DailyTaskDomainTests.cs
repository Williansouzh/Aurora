using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Aurora.Tests.Domain;

public class DailyTaskDomainTests
{
    [Fact]
    public void Complete_deve_marcar_tarefa_como_concluida()
    {
        var task = new DailyTask { Status = DailyTaskStatus.Pending };
        task.Complete();
        task.Status.Should().Be(DailyTaskStatus.Completed);
        task.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_em_tarefa_ja_concluida_deve_lancar_excecao()
    {
        var task = new DailyTask { Status = DailyTaskStatus.Completed };
        task.Invoking(t => t.Complete())
            .Should().Throw<Aurora.Domain.Exceptions.DomainException>();
    }

    [Fact]
    public void Reopen_deve_reverter_tarefa_para_pendente()
    {
        var task = new DailyTask { Status = DailyTaskStatus.Completed, CompletedAt = DateTime.UtcNow };
        task.Reopen();
        task.Status.Should().Be(DailyTaskStatus.Pending);
        task.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Reopen_em_tarefa_pendente_deve_lancar_excecao()
    {
        var task = new DailyTask { Status = DailyTaskStatus.Pending };
        task.Invoking(t => t.Reopen())
            .Should().Throw<Aurora.Domain.Exceptions.DomainException>();
    }
}
