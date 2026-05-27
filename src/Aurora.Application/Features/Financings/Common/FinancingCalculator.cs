using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;

namespace Aurora.Application.Features.Financings.Common;

public static class FinancingCalculator
{
    public static FinancingSimulationDto Simulate(
        decimal assetValue,
        decimal downPayment,
        AmortizationSystem system,
        decimal annualInterestRate,
        decimal monthlyInsurance,
        decimal monthlyFees,
        int termMonths,
        DateTime firstDueDate)
    {
        var financedAmount = assetValue - downPayment;
        var installments = BuildSchedule(financedAmount, system, annualInterestRate, monthlyInsurance, monthlyFees, termMonths, firstDueDate);
        var total = installments.Sum(i => i.TotalPayment);
        var totalInterest = installments.Sum(i => i.Interest);
        var interestShare = total == 0 ? 0 : Math.Round((totalInterest / total) * 100, 2);

        return new FinancingSimulationDto(
            financedAmount,
            installments.First().TotalPayment,
            installments.Last().TotalPayment,
            total,
            totalInterest,
            total / termMonths,
            interestShare,
            installments.Select(i => i.ToDto()).ToList());
    }

    public static List<FinancingInstallment> BuildSchedule(
        decimal financedAmount,
        AmortizationSystem system,
        decimal annualInterestRate,
        decimal monthlyInsurance,
        decimal monthlyFees,
        int termMonths,
        DateTime firstDueDate)
    {
        if (financedAmount <= 0) throw new ValidationException("Valor financiado deve ser positivo");
        if (termMonths <= 0) throw new ValidationException("Prazo deve ser positivo");
        if (annualInterestRate < 0) throw new ValidationException("Taxa de juros nao pode ser negativa");

        var monthlyRate = annualInterestRate / 100m / 12m;

        return system == AmortizationSystem.SAC
            ? BuildSac(financedAmount, monthlyRate, monthlyInsurance, monthlyFees, termMonths, firstDueDate)
            : BuildPrice(financedAmount, monthlyRate, monthlyInsurance, monthlyFees, termMonths, firstDueDate);
    }

    private static List<FinancingInstallment> BuildSac(
        decimal principal,
        decimal monthlyRate,
        decimal insurance,
        decimal fees,
        int termMonths,
        DateTime firstDueDate)
    {
        var rows = new List<FinancingInstallment>();
        var balance = principal;
        var amortization = Round(principal / termMonths);

        for (var n = 1; n <= termMonths; n++)
        {
            var opening = balance;
            var interest = Round(opening * monthlyRate);
            var currentAmortization = n == termMonths ? opening : amortization;
            balance = Round(opening - currentAmortization);
            rows.Add(Row(n, firstDueDate, opening, currentAmortization, interest, insurance, fees, balance));
        }
        return rows;
    }

    private static List<FinancingInstallment> BuildPrice(
        decimal principal,
        decimal monthlyRate,
        decimal insurance,
        decimal fees,
        int termMonths,
        DateTime firstDueDate)
    {
        var rows = new List<FinancingInstallment>();
        var balance = principal;
        var basePayment = monthlyRate == 0
            ? principal / termMonths
            : principal * monthlyRate / (1 - (decimal)Math.Pow((double)(1 + monthlyRate), -termMonths));
        basePayment = Round(basePayment);

        for (var n = 1; n <= termMonths; n++)
        {
            var opening = balance;
            var interest = Round(opening * monthlyRate);
            var amortization = n == termMonths ? opening : Round(basePayment - interest);
            balance = Round(opening - amortization);
            rows.Add(Row(n, firstDueDate, opening, amortization, interest, insurance, fees, balance));
        }
        return rows;
    }

    private static FinancingInstallment Row(
        int number,
        DateTime firstDueDate,
        decimal opening,
        decimal amortization,
        decimal interest,
        decimal insurance,
        decimal fees,
        decimal closing) =>
        new()
        {
            Number = number,
            DueDate = firstDueDate.Date.AddMonths(number - 1),
            OpeningBalance = opening,
            Amortization = amortization,
            Interest = interest,
            Insurance = insurance,
            Fees = fees,
            TotalPayment = Round(amortization + interest + insurance + fees),
            ClosingBalance = closing,
            Status = FinancingInstallmentStatus.Planned
        };

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
