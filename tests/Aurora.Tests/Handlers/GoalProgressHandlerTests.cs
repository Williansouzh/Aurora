using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Goals.UpdateProgress;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Aurora.Tests.Handlers;

public class GoalProgressHandlerTests
{
    private readonly Mock<IGoalRepository> _goalRepo = new();
    private readonly Mock<ITimelineEventRepository> _timelineRepo = new();

    private UpdateGoalProgressHandler CreateHandler() =>
        new(_goalRepo.Object, _timelineRepo.Object);

    private static Goal NumericGoal(decimal target) => new()
    {
        Id = "g1", UserId = "u1", Title = "Poupar",
        Status = GoalStatus.Active, MetricType = GoalMetricType.Numeric, TargetValue = target,
    };

    [Fact]
    public async Task Deve_atualizar_progress_e_manter_status_ativo()
    {
        var goal = NumericGoal(1000);
        _goalRepo.Setup(r => r.GetByIdAsync("g1", "u1", default)).ReturnsAsync(goal);
        _goalRepo.Setup(r => r.UpdateAsync(It.IsAny<Goal>(), default)).Returns(Task.CompletedTask);
        _timelineRepo.Setup(r => r.AddFromModuleAsync(It.IsAny<TimelineEvent>())).Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(new UpdateGoalProgressCommand("u1", "g1", 500), default);

        result.CurrentValue.Should().Be(500);
        result.Status.Should().Be(GoalStatus.Active);
    }

    [Fact]
    public async Task Deve_completar_meta_quando_progress_atinge_alvo()
    {
        var goal = NumericGoal(1000);
        _goalRepo.Setup(r => r.GetByIdAsync("g1", "u1", default)).ReturnsAsync(goal);
        _goalRepo.Setup(r => r.UpdateAsync(It.IsAny<Goal>(), default)).Returns(Task.CompletedTask);
        _timelineRepo.Setup(r => r.AddFromModuleAsync(It.IsAny<TimelineEvent>())).Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(new UpdateGoalProgressCommand("u1", "g1", 1000), default);

        result.Status.Should().Be(GoalStatus.Completed);
    }

    [Fact]
    public async Task Deve_emitir_evento_GoalCompleted_na_timeline_quando_concluir()
    {
        var goal = NumericGoal(100);
        _goalRepo.Setup(r => r.GetByIdAsync("g1", "u1", default)).ReturnsAsync(goal);
        _goalRepo.Setup(r => r.UpdateAsync(It.IsAny<Goal>(), default)).Returns(Task.CompletedTask);
        _timelineRepo.Setup(r => r.AddFromModuleAsync(It.IsAny<TimelineEvent>())).Returns(Task.CompletedTask);

        await CreateHandler().Handle(new UpdateGoalProgressCommand("u1", "g1", 100), default);

        _timelineRepo.Verify(r => r.AddFromModuleAsync(
            It.Is<TimelineEvent>(e => e.Type == TimelineEventType.GoalCompleted)), Times.Once);
    }

    [Fact]
    public async Task Deve_lancar_excecao_se_meta_nao_existir()
    {
        _goalRepo.Setup(r => r.GetByIdAsync("nao-existe", "u1", default)).ReturnsAsync((Goal?)null);

        var act = () => CreateHandler().Handle(new UpdateGoalProgressCommand("u1", "nao-existe", 50), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
