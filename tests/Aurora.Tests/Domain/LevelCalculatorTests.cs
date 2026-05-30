using Aurora.Domain.Common;
using FluentAssertions;
using Xunit;

namespace Aurora.Tests.Domain;

public class LevelCalculatorTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(99, 1)]
    [InlineData(100, 2)]
    [InlineData(299, 2)]
    [InlineData(300, 3)]
    [InlineData(1000, 5)]
    [InlineData(4500, 10)]
    public void Compute_deve_retornar_nivel_correto(int xp, int expectedLevel)
    {
        LevelCalculator.Compute(xp).Should().Be(expectedLevel);
    }

    [Fact]
    public void Nivel_maximo_eh_50()
    {
        LevelCalculator.Compute(999_999).Should().Be(50);
    }

    [Theory]
    [InlineData(1, "Início")]
    [InlineData(5, "Organizado")]
    [InlineData(10, "Consistente")]
    [InlineData(20, "Focado")]
    [InlineData(30, "Em evolução")]
    [InlineData(50, "Modo Aurora")]
    public void LevelName_deve_retornar_nome_correto(int level, string expectedName)
    {
        LevelCalculator.LevelName(level).Should().Be(expectedName);
    }

    [Fact]
    public void XpToNext_deve_ser_zero_no_nivel_maximo()
    {
        LevelCalculator.XpToNext(999_999).Should().Be(0);
    }

    [Fact]
    public void XpToNext_deve_ser_positivo_antes_do_nivel_maximo()
    {
        LevelCalculator.XpToNext(0).Should().BePositive();
    }
}
