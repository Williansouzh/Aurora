using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Transactions.Common;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Transactions.GetPaged;

public record GetTransactionsQuery(
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
    decimal? MaxAmount = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResultDto<TransactionDto>>;

public class GetTransactionsHandler(ITransactionRepository transactions)
    : IRequestHandler<GetTransactionsQuery, PagedResultDto<TransactionDto>>
{
    public async Task<PagedResultDto<TransactionDto>> Handle(GetTransactionsQuery query, CancellationToken ct)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize > 200 ? 200 : query.PageSize;

        var filter = new TransactionFilter(
            query.UserId, query.Month, query.Year, query.Type, query.Status,
            query.CategoryId, query.AccountId, query.Search, query.DateFrom, query.DateTo,
            query.AccountIds, query.CategoryIds, query.MinAmount, query.MaxAmount);

        var (items, total) = await transactions.GetPagedAsync(filter, page, pageSize);
        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

        return new PagedResultDto<TransactionDto>(
            items.Select(x => x.ToDto()).ToList(), total, page, pageSize, totalPages);
    }
}
