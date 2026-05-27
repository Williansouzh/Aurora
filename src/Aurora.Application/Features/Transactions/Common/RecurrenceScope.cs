using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;

namespace Aurora.Application.Features.Transactions.Common;

public static class RecurrenceScope
{
    public static async Task<List<Transaction>> SelectAsync(
        ITransactionRepository repo,
        Transaction selected,
        string userId,
        string scope)
    {
        if (!selected.RecurrenceGroupId.HasValue || scope == "this") return [selected];

        var group = await repo.GetByRecurrenceGroupAsync(selected.RecurrenceGroupId.Value, userId);
        return scope switch
        {
            "future" => group.Where(x => x.Date >= selected.Date).ToList(),
            "all" => group,
            _ => [selected]
        };
    }
}
