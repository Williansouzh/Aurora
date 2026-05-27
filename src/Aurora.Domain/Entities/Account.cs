using Aurora.Domain.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;

namespace Aurora.Domain.Entities;

public class Account : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal InitialBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Color { get; set; } = "#6366f1";
    public bool IsArchived { get; set; }
    public decimal CreditLimit { get; set; }
    public int ClosingDay { get; set; } = 10;
    public int DueDay { get; set; } = 15;

    public void Debit(decimal amount)
    {
        EnsurePositive(amount);
        CurrentBalance -= amount;
    }

    public void Credit(decimal amount)
    {
        EnsurePositive(amount);
        CurrentBalance += amount;
    }

    public void ApplyTransaction(TransactionType type, decimal amount)
    {
        EnsurePositive(amount);

        if (type == TransactionType.Income)
        {
            Credit(amount);
            return;
        }

        Debit(amount);
    }

    public void ReverseTransaction(TransactionType type, decimal amount)
    {
        EnsurePositive(amount);

        if (type == TransactionType.Income)
        {
            Debit(amount);
            return;
        }

        Credit(amount);
    }

    public void ReplaceCurrentBalance(decimal amount)
    {
        if (amount < 0)
        {
            throw new ValidationException("Account balance cannot be negative");
        }

        CurrentBalance = amount;
    }

    private static void EnsurePositive(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ValidationException("Amount must be positive");
        }
    }
}
