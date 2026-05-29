using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Transactions.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Transactions.UpdateRecurring;

public record UpdateRecurringTransactionCommand(
    string UserId,
    string Id,
    string Scope,
    string AccountId,
    string CategoryId,
    string Description,
    decimal Amount,
    TransactionType Type,
    TransactionStatus Status,
    DateTime Date,
    DateTime? DueDate,
    string? Notes,
    bool IsRecurring = false,
    RecurrenceType? RecurrenceType = null,
    int RecurrenceInterval = 1,
    DateTime? RecurrenceEndDate = null,
    int? TotalInstallments = null) : IRequest<List<TransactionDto>>;

public class UpdateRecurringTransactionHandler(
    ITransactionRepository txRepo,
    IAccountRepository accRepo,
    ICategoryRepository catRepo,
    ICacheService cache) : IRequestHandler<UpdateRecurringTransactionCommand, List<TransactionDto>>
{
    public async Task<List<TransactionDto>> Handle(UpdateRecurringTransactionCommand command, CancellationToken ct)
    {
        var selected = await txRepo.GetByIdAsync(command.Id, command.UserId)
            ?? throw new NotFoundException("Transacao nao encontrada");

        var targets = await RecurrenceScope.SelectAsync(txRepo, selected, command.UserId, command.Scope);
        var fallbackType = command.RecurrenceType ?? Domain.Enums.RecurrenceType.Monthly;

        foreach (var tx in targets)
        {
            tx.AccountId = command.AccountId;
            tx.CategoryId = command.CategoryId;
            tx.Description = tx.TotalInstallments.HasValue && tx.RecurrenceIndex.HasValue
                ? $"{StripInstallmentSuffix(command.Description)} ({tx.RecurrenceIndex}/{tx.TotalInstallments})"
                : command.Description;
            tx.Amount = command.Amount;
            tx.Type = command.Type;
            tx.Status = command.Status;
            tx.DueDate = command.DueDate.HasValue
                ? RecurrenceGenerator.AddInterval(
                    command.DueDate.Value,
                    command.RecurrenceType ?? tx.RecurrenceType ?? fallbackType,
                    command.RecurrenceInterval > 0 ? command.RecurrenceInterval : tx.RecurrenceInterval,
                    (tx.RecurrenceIndex ?? 1) - 1)
                : null;
            tx.Notes = command.Notes;
            tx.IsRecurring = command.IsRecurring || tx.IsRecurring;
            tx.RecurrenceType = command.RecurrenceType ?? tx.RecurrenceType;
            tx.RecurrenceInterval = Math.Max(1, command.RecurrenceInterval);
            tx.RecurrenceEndDate = command.RecurrenceEndDate;
            tx.TotalInstallments = command.TotalInstallments ?? tx.TotalInstallments;

            if (!string.IsNullOrWhiteSpace(tx.CategoryId))
            {
                _ = await catRepo.GetByIdAsync(tx.CategoryId, command.UserId)
                    ?? throw new ValidationException("Categoria invalida");
            }
            _ = await accRepo.GetByIdAsync(tx.AccountId, command.UserId)
                ?? throw new ValidationException("Conta invalida");

            await txRepo.UpdateAsync(tx);
        }

        await cache.RemoveByPrefixAsync(CacheKeys.DashboardPrefix(command.UserId), ct);
        return targets.Select(x => x.ToDto()).ToList();
    }

    private static string StripInstallmentSuffix(string value)
    {
        var idx = value.LastIndexOf(" (", StringComparison.Ordinal);
        return idx > 0 ? value[..idx] : value;
    }
}
