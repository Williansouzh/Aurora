using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Aurora.Tests.Domain;

public class GoalDomainTests
{
    private static Goal ActiveGoal(GoalMetricType metric = GoalMetricType.None, decimal target = 0) =>
        new() { UserId = "u1", Title = "Meta", Status = GoalStatus.Active, MetricType = metric, TargetValue = target };

    [Fact]
    public void AddMilestone_deve_adicionar_milestone_na_lista()
    {
        var goal = ActiveGoal();
        var m = goal.AddMilestone("Passo 1", isRequired: true);
        goal.Milestones.Should().ContainSingle();
        m.Title.Should().Be("Passo 1");
        m.IsRequired.Should().BeTrue();
    }

    [Fact]
    public void CompleteMilestone_deve_marcar_milestone_como_concluido()
    {
        var goal = ActiveGoal();
        var m = goal.AddMilestone("Etapa", isRequired: true);
        goal.CompleteMilestone(m.Id);
        m.IsCompleted.Should().BeTrue();
        m.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void CompleteMilestone_de_id_inexistente_deve_lancar_excecao()
    {
        var goal = ActiveGoal();
        goal.Invoking(g => g.CompleteMilestone("nao-existe"))
            .Should().Throw<Aurora.Domain.Exceptions.NotFoundException>();
    }

    [Fact]
    public void Meta_deve_completar_automaticamente_quando_todos_milestones_obrigatorios_concluidos()
    {
        var goal = ActiveGoal();
        var m1 = goal.AddMilestone("M1", isRequired: true);
        var m2 = goal.AddMilestone("M2", isRequired: true);

        goal.CompleteMilestone(m1.Id);
        goal.Status.Should().Be(GoalStatus.Active);

        goal.CompleteMilestone(m2.Id);
        goal.Status.Should().Be(GoalStatus.Completed);
    }

    [Fact]
    public void Meta_nao_deve_completar_se_houver_milestone_opcional_pendente()
    {
        var goal = ActiveGoal();
        var required = goal.AddMilestone("Obrigatório", isRequired: true);
        goal.AddMilestone("Opcional", isRequired: false);

        goal.CompleteMilestone(required.Id);
        goal.Status.Should().Be(GoalStatus.Completed);
    }

    [Fact]
    public void ForceComplete_sem_motivo_deve_lancar_excecao()
    {
        var goal = ActiveGoal();
        goal.Invoking(g => g.ForceComplete(""))
            .Should().Throw<Aurora.Domain.Exceptions.ValidationException>();
    }

    [Fact]
    public void ForceComplete_com_motivo_deve_completar_meta()
    {
        var goal = ActiveGoal();
        goal.ForceComplete("Objetivo atingido antes do prazo");
        goal.Status.Should().Be(GoalStatus.Completed);
    }

    [Fact]
    public void Progress_com_metrica_numerica_deve_calcular_percentual()
    {
        var goal = ActiveGoal(GoalMetricType.Numeric, target: 100);
        goal.CurrentValue = 50;
        goal.Progress.Should().Be(50);
    }

    [Fact]
    public void Progress_deve_ser_limitado_a_100()
    {
        var goal = ActiveGoal(GoalMetricType.Numeric, target: 100);
        goal.CurrentValue = 200;
        goal.Progress.Should().Be(100);
    }

    [Fact]
    public void UpdateProgress_em_meta_sem_metrica_deve_lancar_excecao()
    {
        var goal = ActiveGoal(GoalMetricType.None);
        goal.Invoking(g => g.UpdateProgress(10))
            .Should().Throw<Aurora.Domain.Exceptions.DomainException>();
    }
}
