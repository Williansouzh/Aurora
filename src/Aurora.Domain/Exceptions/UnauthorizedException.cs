namespace Aurora.Domain.Exceptions;

public class UnauthorizedException(string message) : DomainException(message);
