using Aurora.Application.Features.Accounts.Archive;
using Aurora.Application.Features.Accounts.Delete;
using Aurora.Application.Features.Accounts.Update;
using Aurora.Application.Features.Auth.Logout;
using Aurora.Application.Features.Auth.Refresh;
using Aurora.Application.Features.Auth.UpdatePassword;
using Aurora.Application.Features.Auth.UpdateProfile;
using Aurora.Application.Features.Budgets.Delete;
using Aurora.Application.Features.Budgets.Upsert;
using Aurora.Application.Features.Categories.Create;
using Aurora.Application.Features.Categories.Delete;
using Aurora.Application.Features.Categories.Update;
using Aurora.Application.Features.CreditCardInvoices.Pay;
using Aurora.Application.Features.Financings.Compare;
using Aurora.Application.Features.Financings.Create;
using Aurora.Application.Features.Financings.Delete;
using Aurora.Application.Features.Financings.LinkTransaction;
using Aurora.Application.Features.Financings.MarkInstallmentAsPaid;
using Aurora.Application.Features.Financings.Simulate;
using Aurora.Application.Features.Financings.SimulateExtraAmortization;
using Aurora.Application.Features.Financings.Update;
using Aurora.Application.Features.Transactions.Delete;
using Aurora.Application.Features.Transactions.DeleteRecurring;
using Aurora.Application.Features.Transactions.MarkAsPaid;
using Aurora.Application.Features.Transactions.MarkAsPending;
using Aurora.Application.Features.Transactions.UpdateRecurring;
using Aurora.Application.Features.Transfers.Create;
using Aurora.Application.Features.Transfers.Delete;
using Aurora.Domain.Enums;
using FluentValidation;

namespace Aurora.Application.Features;

public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Color).NotEmpty().MaximumLength(32);
        When(x => x.Type == AccountType.CreditCard, () =>
        {
            RuleFor(x => x.CreditLimit).GreaterThan(0);
            RuleFor(x => x.ClosingDay).InclusiveBetween(1, 28);
            RuleFor(x => x.DueDay).InclusiveBetween(1, 28);
        });
    }
}

public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator() => this.RuleForIds();
}

public class ArchiveAccountCommandValidator : AbstractValidator<ArchiveAccountCommand>
{
    public ArchiveAccountCommandValidator() => this.RuleForIds();
}

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Color).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Icon).NotEmpty().MaximumLength(64);
    }
}

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Color).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Icon).NotEmpty().MaximumLength(64);
    }
}

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator() => this.RuleForIds();
}

public class UpsertBudgetCommandValidator : AbstractValidator<UpsertBudgetCommand>
{
    public UpsertBudgetCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.LimitAmount).GreaterThan(0);
    }
}

public class DeleteBudgetCommandValidator : AbstractValidator<DeleteBudgetCommand>
{
    public DeleteBudgetCommandValidator() => this.RuleForIds();
}

public class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FromAccountId).NotEmpty();
        RuleFor(x => x.ToAccountId).NotEmpty().NotEqual(x => x.FromAccountId);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
    }
}

public class DeleteTransferCommandValidator : AbstractValidator<DeleteTransferCommand>
{
    public DeleteTransferCommandValidator() => this.RuleForIds();
}

public class DeleteTransactionCommandValidator : AbstractValidator<DeleteTransactionCommand>
{
    public DeleteTransactionCommandValidator() => this.RuleForIds();
}

public class MarkTransactionAsPaidCommandValidator : AbstractValidator<MarkTransactionAsPaidCommand>
{
    public MarkTransactionAsPaidCommandValidator() => this.RuleForIds();
}

public class MarkTransactionAsPendingCommandValidator : AbstractValidator<MarkTransactionAsPendingCommand>
{
    public MarkTransactionAsPendingCommandValidator() => this.RuleForIds();
}

public class DeleteRecurringTransactionCommandValidator : AbstractValidator<DeleteRecurringTransactionCommand>
{
    public DeleteRecurringTransactionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Scope).NotEmpty().Must(BeValidScope);
    }

    private static bool BeValidScope(string scope) =>
        new[] { "single", "future", "all" }.Contains(scope, StringComparer.OrdinalIgnoreCase);
}

public class UpdateRecurringTransactionCommandValidator : AbstractValidator<UpdateRecurringTransactionCommand>
{
    public UpdateRecurringTransactionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Scope).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.RecurrenceInterval).GreaterThan(0);
        RuleFor(x => x.TotalInstallments).GreaterThan(0).When(x => x.TotalInstallments.HasValue);
    }
}

public class PayCreditCardInvoiceCommandValidator : AbstractValidator<PayCreditCardInvoiceCommand>
{
    public PayCreditCardInvoiceCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SourceAccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
    }
}

public class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand>
{
    public UpdatePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(10);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword);
    }
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.RawToken).NotEmpty();
}

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator() => RuleFor(x => x.RawToken).NotEmpty();
}

public class CreateFinancingCommandValidator : AbstractValidator<CreateFinancingCommand>
{
    public CreateFinancingCommandValidator() => this.AddFinancingRules();
}

public class UpdateFinancingCommandValidator : AbstractValidator<UpdateFinancingCommand>
{
    public UpdateFinancingCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
        this.AddFinancingRules();
    }
}

public class DeleteFinancingCommandValidator : AbstractValidator<DeleteFinancingCommand>
{
    public DeleteFinancingCommandValidator() => this.RuleForIds();
}

public class SimulateFinancingCommandValidator : AbstractValidator<SimulateFinancingCommand>
{
    public SimulateFinancingCommandValidator() => this.AddSimulationRules();
}

public class CompareFinancingCommandValidator : AbstractValidator<CompareFinancingCommand>
{
    public CompareFinancingCommandValidator() => this.AddComparisonRules();
}

public class SimulateExtraAmortizationCommandValidator : AbstractValidator<SimulateExtraAmortizationCommand>
{
    public SimulateExtraAmortizationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FinancingId).NotEmpty();
        RuleFor(x => x.ExtraAmount).GreaterThan(0);
    }
}

public class MarkFinancingInstallmentAsPaidCommandValidator
    : AbstractValidator<MarkFinancingInstallmentAsPaidCommand>
{
    public MarkFinancingInstallmentAsPaidCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FinancingId).NotEmpty();
        RuleFor(x => x.Number).GreaterThan(0);
        RuleFor(x => x.PaidAmount).GreaterThan(0).When(x => x.PaidAmount.HasValue);
    }
}

public class LinkTransactionCommandValidator : AbstractValidator<LinkTransactionCommand>
{
    public LinkTransactionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FinancingId).NotEmpty();
        RuleFor(x => x.Number).GreaterThan(0);
        RuleFor(x => x.TransactionId).NotEmpty();
    }
}

internal static class ValidatorExtensions
{
    public static void RuleForIds<T>(this AbstractValidator<T> validator)
    {
        validator.RuleFor(command => GetProperty(command!, "UserId")).NotEmpty();
        validator.RuleFor(command => GetProperty(command!, "Id")).NotEmpty();
    }

    public static void AddFinancingRules<T>(this AbstractValidator<T> validator)
        where T : CreateFinancingCommand
    {
        validator.RuleFor(x => x.UserId).NotEmpty();
        validator.RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        validator.RuleFor(x => x.Type).IsInEnum();
        validator.RuleFor(x => x.AmortizationSystem).IsInEnum();
        validator.RuleFor(x => x.Institution).NotEmpty().MaximumLength(120);
        validator.RuleFor(x => x.AssetValue).GreaterThan(0);
        validator.RuleFor(x => x.DownPayment).GreaterThanOrEqualTo(0).LessThan(x => x.AssetValue);
        validator.RuleFor(x => x.AnnualInterestRate).GreaterThanOrEqualTo(0);
        validator.RuleFor(x => x.MonthlyInsurance).GreaterThanOrEqualTo(0);
        validator.RuleFor(x => x.MonthlyFees).GreaterThanOrEqualTo(0);
        validator.RuleFor(x => x.TermMonths).InclusiveBetween(1, 600);
    }

    public static void AddFinancingRules(this AbstractValidator<UpdateFinancingCommand> validator)
    {
        validator.RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        validator.RuleFor(x => x.Type).IsInEnum();
        validator.RuleFor(x => x.AmortizationSystem).IsInEnum();
        validator.RuleFor(x => x.Institution).NotEmpty().MaximumLength(120);
        validator.RuleFor(x => x.AssetValue).GreaterThan(0);
        validator.RuleFor(x => x.DownPayment).GreaterThanOrEqualTo(0).LessThan(x => x.AssetValue);
        validator.RuleFor(x => x.AnnualInterestRate).GreaterThanOrEqualTo(0);
        validator.RuleFor(x => x.MonthlyInsurance).GreaterThanOrEqualTo(0);
        validator.RuleFor(x => x.MonthlyFees).GreaterThanOrEqualTo(0);
        validator.RuleFor(x => x.TermMonths).InclusiveBetween(1, 600);
    }

    public static void AddSimulationRules(this AbstractValidator<SimulateFinancingCommand> validator)
    {
        validator.RuleFor(x => x.AssetValue).GreaterThan(0);
        validator.RuleFor(x => x.DownPayment).GreaterThanOrEqualTo(0).LessThan(x => x.AssetValue);
        validator.RuleFor(x => x.AmortizationSystem).IsInEnum();
        validator.RuleFor(x => x.AnnualInterestRate).GreaterThanOrEqualTo(0);
        validator.RuleFor(x => x.MonthlyInsurance).GreaterThanOrEqualTo(0);
        validator.RuleFor(x => x.MonthlyFees).GreaterThanOrEqualTo(0);
        validator.RuleFor(x => x.TermMonths).InclusiveBetween(1, 600);
    }

    public static void AddComparisonRules(this AbstractValidator<CompareFinancingCommand> validator)
    {
        validator.RuleFor(x => x.AssetValue).GreaterThan(0);
        validator.RuleFor(x => x.DownPayment).GreaterThanOrEqualTo(0).LessThan(x => x.AssetValue);
        validator.RuleFor(x => x.AnnualInterestRate).GreaterThanOrEqualTo(0);
        validator.RuleFor(x => x.MonthlyInsurance).GreaterThanOrEqualTo(0);
        validator.RuleFor(x => x.MonthlyFees).GreaterThanOrEqualTo(0);
        validator.RuleFor(x => x.TermMonths).InclusiveBetween(1, 600);
    }

    private static string? GetProperty<T>(T command, string propertyName) =>
        command?.GetType().GetProperty(propertyName)?.GetValue(command)?.ToString();
}
