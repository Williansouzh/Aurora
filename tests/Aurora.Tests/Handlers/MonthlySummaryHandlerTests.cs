using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Dashboard.Common;
using Aurora.Application.Features.Dashboard.MonthlySummary;
using Aurora.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Aurora.Tests.Handlers;

public class MonthlySummaryHandlerTests
{
    private readonly Mock<IAccountRepository> _accRepo = new();
    private readonly Mock<ITransactionRepository> _txRepo = new();
    private readonly Mock<ICacheService> _cache = new();

    private GetMonthlySummaryHandler CreateHandler() =>
        new(_accRepo.Object, _txRepo.Object, _cache.Object);

    private void SetupCacheMiss()
    {
        _cache.Setup(c => c.GetAsync<MonthlySummaryDto>(It.IsAny<string>(), default))
              .ReturnsAsync((MonthlySummaryDto?)null);
        _cache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<MonthlySummaryDto>(),
              It.IsAny<TimeSpan>(), default)).Returns(Task.CompletedTask);
    }

    private void SetupDefaultRepos(decimal income = 0, decimal expense = 0,
        decimal prevIncome = 0, decimal prevExpense = 0)
    {
        _accRepo.Setup(r => r.GetTotalBalanceAsync("user1")).ReturnsAsync(5000m);
        _txRepo.Setup(r => r.SumAsync("user1", 5, 2025, TransactionType.Income, TransactionStatus.Paid)).ReturnsAsync(income);
        _txRepo.Setup(r => r.SumAsync("user1", 5, 2025, TransactionType.Expense, TransactionStatus.Paid)).ReturnsAsync(expense);
        _txRepo.Setup(r => r.SumAsync("user1", 4, 2025, TransactionType.Income, TransactionStatus.Paid)).ReturnsAsync(prevIncome);
        _txRepo.Setup(r => r.SumAsync("user1", 4, 2025, TransactionType.Expense, TransactionStatus.Paid)).ReturnsAsync(prevExpense);
        _txRepo.Setup(r => r.SumAsync("user1", 5, 2025, TransactionType.Income, TransactionStatus.Pending)).ReturnsAsync(0m);
        _txRepo.Setup(r => r.SumAsync("user1", 5, 2025, TransactionType.Expense, TransactionStatus.Pending)).ReturnsAsync(0m);
        _txRepo.Setup(r => r.CountAsync("user1", 5, 2025, TransactionStatus.Paid)).ReturnsAsync(0);
        _txRepo.Setup(r => r.CountAsync("user1", 5, 2025, TransactionStatus.Pending)).ReturnsAsync(0);
        _txRepo.Setup(r => r.RecentAsync("user1", It.IsAny<int>())).ReturnsAsync([]);
        _txRepo.Setup(r => r.UpcomingDueAsync("user1", It.IsAny<int>())).ReturnsAsync([]);
    }

    [Fact]
    public async Task Deve_somar_apenas_transacoes_pagas_em_receitas_e_despesas()
    {
        SetupCacheMiss();
        SetupDefaultRepos(income: 3000m, expense: 1200m);

        var result = await CreateHandler().Handle(new GetMonthlySummaryQuery("user1", 5, 2025), default);

        result.MonthlyIncome.Should().Be(3000m);
        result.MonthlyExpense.Should().Be(1200m);
        result.MonthlyResult.Should().Be(1800m);
        // Confirm Pending sums are fetched separately (not mixed into income/expense)
        _txRepo.Verify(r => r.SumAsync("user1", 5, 2025, TransactionType.Income, TransactionStatus.Paid), Times.Once);
        _txRepo.Verify(r => r.SumAsync("user1", 5, 2025, TransactionType.Expense, TransactionStatus.Paid), Times.Once);
    }

    [Fact]
    public async Task Deve_retornar_zeros_se_nao_houver_transacoes()
    {
        SetupCacheMiss();
        SetupDefaultRepos();

        var result = await CreateHandler().Handle(new GetMonthlySummaryQuery("user1", 5, 2025), default);

        result.MonthlyIncome.Should().Be(0m);
        result.MonthlyExpense.Should().Be(0m);
        result.MonthlyResult.Should().Be(0m);
        result.IncomeVariation.Should().Be(0m);
        result.ExpenseVariation.Should().Be(0m);
        result.SavingsRate.Should().Be(0m);
    }

    [Fact]
    public async Task Deve_calcular_variacao_em_relacao_ao_mes_anterior_corretamente()
    {
        SetupCacheMiss();
        // Current: income=2000, expense=1000 | Prev: income=1000, expense=500
        SetupDefaultRepos(income: 2000m, expense: 1000m, prevIncome: 1000m, prevExpense: 500m);

        var result = await CreateHandler().Handle(new GetMonthlySummaryQuery("user1", 5, 2025), default);

        result.IncomeVariation.Should().Be(100m);   // (2000-1000)/1000 * 100
        result.ExpenseVariation.Should().Be(100m);  // (1000-500)/500 * 100
    }

    [Fact]
    public async Task Deve_usar_cache_quando_disponivel()
    {
        var cached = new MonthlySummaryDto(5000m, 3000m, 1000m, 2000m, 0, 0, 5, 2, [], 2500m, 800m, 20m, 25m, 66.67m, []);
        _cache.Setup(c => c.GetAsync<MonthlySummaryDto>(It.IsAny<string>(), default))
              .ReturnsAsync(cached);

        var result = await CreateHandler().Handle(new GetMonthlySummaryQuery("user1", 5, 2025), default);

        result.Should().Be(cached);
        _txRepo.Verify(r => r.SumAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<TransactionType>(), It.IsAny<TransactionStatus>()), Times.Never);
    }
}
