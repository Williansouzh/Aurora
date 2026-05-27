using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Transactions.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;
using ValidationException = Aurora.Domain.Exceptions.ValidationException;

namespace Aurora.Application.Features.Transactions.Update;

public record UpdateTransactionCommand(
    string UserId,
    string Id,
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
    int? TotalInstallments = null) : IRequest<TransactionDto>;

public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class UpdateTransactionHandler(
    ITransactionRepository txRepo,
    IAccountRepository accRepo,
    ICategoryRepository catRepo,
    ICacheService cache) : IRequestHandler<UpdateTransactionCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(UpdateTransactionCommand command, CancellationToken ct)
    {
        if (command.Amount <= 0) throw new ValidationException("Amount deve ser positivo");

        var tx = await txRepo.GetByIdAsync(command.Id, command.UserId)
            ?? throw new NotFoundException("Transacao nao encontrada");

        var oldAcc = await accRepo.GetByIdAsync(tx.AccountId, command.UserId)
            ?? throw new ValidationException("Conta antiga invalida");
        var newAcc = await accRepo.GetByIdAsync(command.AccountId, command.UserId)
            ?? throw new ValidationException("Conta invalida");
        _ = await catRepo.GetByIdAsync(command.CategoryId, command.UserId)
            ?? throw new ValidationException("Categoria invalida");

        if (tx.Status == TransactionStatus.Paid && oldAcc.Type != AccountType.CreditCard)
        {
            oldAcc.CurrentBalance -= TransactionMapper.Impact(tx.Type, tx.Amount);
            await accRepo.UpdateAsync(oldAcc);
        }

        tx.AccountId = command.AccountId;
        tx.CategoryId = command.CategoryId;
        tx.Description = command.Description;
        tx.Amount = command.Amount;
        tx.Type = command.Type;
        tx.Status = command.Status;
        tx.Date = command.Date;
        tx.DueDate = command.DueDate;
        tx.Notes = command.Notes;

        if (tx.Status == TransactionStatus.Paid && newAcc.Type != AccountType.CreditCard)
        {
            newAcc.CurrentBalance += TransactionMapper.Impact(tx.Type, tx.Amount);
            await accRepo.UpdateAsync(newAcc);
        }

        await txRepo.UpdateAsync(tx);
        await cache.RemoveByPrefixAsync($"aurora:dashboard:{command.UserId}", ct);
        return tx.ToDto();
    }
}
