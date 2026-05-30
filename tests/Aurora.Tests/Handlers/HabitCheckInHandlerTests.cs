using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Habits.CheckIn;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Aurora.Tests.Handlers;

public class HabitCheckInHandlerTests
{
    private readonly Mock<IHabitRepository> _habitRepo = new();
    private readonly Mock<IHabitCheckInRepository> _checkInRepo = new();
    private readonly Mock<ITimelineEventRepository> _timelineRepo = new();
    private readonly Mock<IXpService> _xpService = new();

    private HabitCheckInHandler CreateHandler() =>
        new(_habitRepo.Object, _checkInRepo.Object, _timelineRepo.Object, _xpService.Object);

    private static Habit SampleHabit(string id = "h1") => new()
    {
        Id = id, UserId = "u1", Name = "Meditar", IsActive = true,
        Difficulty = HabitDifficulty.Medium, Area = LifeArea.Health, XpReward = Habit.XpForDifficulty(HabitDifficulty.Medium),
    };

    [Fact]
    public async Task Deve_criar_checkin_com_xp_quando_done()
    {
        var habit = SampleHabit();
        _habitRepo.Setup(r => r.GetByIdAsync("h1", "u1", default)).ReturnsAsync(habit);
        _checkInRepo.Setup(r => r.GetByHabitAndDateAsync("h1", "u1", It.IsAny<DateTime>()))
            .ReturnsAsync((HabitCheckIn?)null);
        _checkInRepo.Setup(r => r.GetByHabitAsync("h1", "u1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync([]);
        _checkInRepo.Setup(r => r.AddAsync(It.IsAny<HabitCheckIn>(), default)).Returns(Task.CompletedTask);
        _habitRepo.Setup(r => r.UpdateAsync(It.IsAny<Habit>(), default)).Returns(Task.CompletedTask);
        _timelineRepo.Setup(r => r.AddFromModuleAsync(It.IsAny<TimelineEvent>())).Returns(Task.CompletedTask);
        _xpService.Setup(x => x.AwardAsync(It.IsAny<string>(), It.IsAny<XpSource>(), It.IsAny<int>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        var cmd = new HabitCheckInCommand("u1", "h1", DateTime.UtcNow, HabitCheckInStatus.Done, null, null);
        var result = await CreateHandler().Handle(cmd, default);

        result.XpGenerated.Should().Be(Habit.XpForDifficulty(HabitDifficulty.Medium));
        _xpService.Verify(x => x.AwardAsync("u1", XpSource.HabitCheckIn, It.IsAny<int>(), It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task Deve_lancar_excecao_para_checkin_duplicado()
    {
        var habit = SampleHabit();
        _habitRepo.Setup(r => r.GetByIdAsync("h1", "u1", default)).ReturnsAsync(habit);
        _checkInRepo.Setup(r => r.GetByHabitAndDateAsync("h1", "u1", It.IsAny<DateTime>()))
            .ReturnsAsync(new HabitCheckIn { Id = "existing", HabitId = "h1", UserId = "u1" });

        var cmd = new HabitCheckInCommand("u1", "h1", DateTime.UtcNow, HabitCheckInStatus.Done, null, null);
        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Nao_deve_gerar_xp_quando_skip()
    {
        var habit = SampleHabit();
        _habitRepo.Setup(r => r.GetByIdAsync("h1", "u1", default)).ReturnsAsync(habit);
        _checkInRepo.Setup(r => r.GetByHabitAndDateAsync("h1", "u1", It.IsAny<DateTime>()))
            .ReturnsAsync((HabitCheckIn?)null);
        _checkInRepo.Setup(r => r.AddAsync(It.IsAny<HabitCheckIn>(), default)).Returns(Task.CompletedTask);
        _timelineRepo.Setup(r => r.AddFromModuleAsync(It.IsAny<TimelineEvent>())).Returns(Task.CompletedTask);

        var cmd = new HabitCheckInCommand("u1", "h1", DateTime.UtcNow, HabitCheckInStatus.Skipped, null, null);
        var result = await CreateHandler().Handle(cmd, default);

        result.XpGenerated.Should().Be(0);
        _xpService.Verify(x => x.AwardAsync(It.IsAny<string>(), It.IsAny<XpSource>(), It.IsAny<int>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task Deve_lancar_excecao_se_habito_nao_existir()
    {
        _habitRepo.Setup(r => r.GetByIdAsync("nao-existe", "u1", default)).ReturnsAsync((Habit?)null);

        var cmd = new HabitCheckInCommand("u1", "nao-existe", DateTime.UtcNow, HabitCheckInStatus.Done, null, null);
        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
