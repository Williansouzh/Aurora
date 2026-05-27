namespace Aurora.Domain.Exceptions;

public class ValidationException(string message, IDictionary<string, string[]>? errors = null)
    : DomainException(message)
{
    public IDictionary<string, string[]> Errors { get; } = errors ?? new Dictionary<string, string[]>();
}
