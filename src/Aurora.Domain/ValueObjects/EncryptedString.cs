using Aurora.Domain.Common;

namespace Aurora.Domain.ValueObjects;

public sealed record EncryptedString(string CipherText) : ValueObject
{
    public override string ToString() => CipherText;
}
