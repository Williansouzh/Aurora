using Aurora.Domain.Common;
using Aurora.Domain.Exceptions;

namespace Aurora.Domain.ValueObjects;

public sealed record Email : ValueObject
{
    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("Email is required");
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!System.Net.Mail.MailAddress.TryCreate(normalized, out var parsed) ||
            parsed.Address != normalized)
        {
            throw new ValidationException("Email is invalid");
        }

        return new Email(normalized);
    }

    public override string ToString() => Value;
}
