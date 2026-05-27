using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.Transfers.Common;

public record TransferDto(
    string Id,
    string FromAccountId,
    string ToAccountId,
    decimal Amount,
    DateTime Date,
    string Description,
    TransferStatus Status);

public static class TransferMapper
{
    public static TransferDto ToDto(this Transfer t) =>
        new(t.Id, t.FromAccountId, t.ToAccountId, t.Amount, t.Date, t.Description, t.Status);
}
