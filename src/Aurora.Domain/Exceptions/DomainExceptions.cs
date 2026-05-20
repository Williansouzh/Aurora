namespace Aurora.Domain.Exceptions;
public class DomainException(string message): Exception(message);
public class NotFoundException(string message): DomainException(message);
public class ConflictException(string message): DomainException(message);
public class ValidationException(string message, IDictionary<string,string[]>? errors=null): DomainException(message){ public IDictionary<string,string[]> Errors { get; } = errors ?? new Dictionary<string,string[]>(); }
