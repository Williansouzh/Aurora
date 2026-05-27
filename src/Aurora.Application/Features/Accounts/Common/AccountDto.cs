using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.Accounts.Common;

public record AccountDto(
    string Id,
    string Name,
    AccountType Type,
    decimal InitialBalance,
    decimal CurrentBalance,
    string Color,
    bool IsArchived,
    decimal CreditLimit,
    int ClosingDay,
    int DueDay,
    decimal AvailableLimit,
    decimal CurrentInvoiceAmount);

public static class AccountMapper
{
    public static AccountDto ToDto(this Account account)
    {
        var available = account.Type == AccountType.CreditCard
            ? Math.Max(0, account.CreditLimit - account.CurrentBalance)
            : account.CurrentBalance;

        return new AccountDto(
            account.Id,
            account.Name,
            account.Type,
            account.InitialBalance,
            account.CurrentBalance,
            account.Color,
            account.IsArchived,
            account.CreditLimit,
            account.ClosingDay,
            account.DueDay,
            available,
            account.Type == AccountType.CreditCard ? account.CurrentBalance : 0);
    }
}
