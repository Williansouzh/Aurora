using Aurora.Domain.Common;
using Aurora.Domain.Exceptions;

namespace Aurora.Domain.ValueObjects;

public sealed record Money : ValueObject
{
    private Money(decimal amount, string currency)
    {
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency;
    }

    public decimal Amount { get; }
    public string Currency { get; }

    public static Money Create(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
        {
            throw new ValidationException("Money amount cannot be negative");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            throw new ValidationException("Currency must be an ISO 4217 code");
        }

        return new Money(amount, currency.Trim().ToUpperInvariant());
    }

    public static Money Positive(decimal amount, string currency = "BRL")
    {
        if (amount <= 0)
        {
            throw new ValidationException("Money amount must be positive");
        }

        return Create(amount, currency);
    }

    public static implicit operator decimal(Money money) => money.Amount;
}
