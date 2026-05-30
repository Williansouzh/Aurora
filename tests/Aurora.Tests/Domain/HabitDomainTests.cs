using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Aurora.Tests.Domain;

public class HabitDomainTests
{
    [Fact]
    public void XpForDifficulty_deve_retornar_valores_corretos()
    {
        Habit.XpForDifficulty(HabitDifficulty.Easy).Should().Be(5);
        Habit.XpForDifficulty(HabitDifficulty.Medium).Should().Be(10);
        Habit.XpForDifficulty(HabitDifficulty.Hard).Should().Be(20);
    }

    [Fact]
    public void Pause_deve_desativar_habito()
    {
        var habit = new Habit { IsActive = true };
        habit.Pause();
        habit.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Pause_em_habito_ja_pausado_deve_lancar_excecao()
    {
        var habit = new Habit { IsActive = false };
        habit.Invoking(h => h.Pause()).Should().Throw<Aurora.Domain.Exceptions.DomainException>();
    }

    [Fact]
    public void Resume_deve_ativar_habito_pausado()
    {
        var habit = new Habit { IsActive = false };
        habit.Resume();
        habit.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateStreak_deve_atualizar_best_streak_quando_atual_maior()
    {
        var habit = new Habit { CurrentStreak = 5, BestStreak = 5 };
        habit.UpdateStreak(10);
        habit.CurrentStreak.Should().Be(10);
        habit.BestStreak.Should().Be(10);
    }

    [Fact]
    public void UpdateStreak_nao_deve_reduzir_best_streak()
    {
        var habit = new Habit { CurrentStreak = 10, BestStreak = 20 };
        habit.UpdateStreak(3);
        habit.CurrentStreak.Should().Be(3);
        habit.BestStreak.Should().Be(20);
    }
}
