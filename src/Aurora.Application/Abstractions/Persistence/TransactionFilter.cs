using Aurora.Domain.Enums;

namespace Aurora.Application.Abstractions.Persistence;

public record TransactionFilter(
    string UserId,
    int? Month,
    int? Year,
    TransactionType? Type,
    TransactionStatus? Status,
    string? CategoryId,
    string? AccountId,
    string? Search = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    List<string>? AccountIds = null,
    List<string>? CategoryIds = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null);
