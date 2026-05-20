namespace Aurora.Application.Common;
public record ApiResponse<T>(bool Success, T? Data, string? Message = null);
public record ErrorResponse(string Message, string TraceId, IDictionary<string, string[]>? Errors = null);
