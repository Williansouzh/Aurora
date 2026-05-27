using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Transactions.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;
using ValidationException = Aurora.Domain.Exceptions.ValidationException;

namespace Aurora.Application.Features.Transactions.Create;

public record CreateTransactionCommand(
    string UserId,
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

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class CreateTransactionHandler(
    ITransactionRepository txRepo,
    IAccountRepository accRepo,
    ICategoryRepository catRepo,
    ICreditCardInvoiceRepository invoiceRepo,
    ICacheService cache) : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(CreateTransactionCommand command, CancellationToken ct)
    {
        if (command.Amount <= 0) throw new ValidationException("Amount deve ser positivo");

        var transactions = command.IsRecurring || command.TotalInstallments.HasValue
            ? RecurrenceGenerator.Generate(command)
            : new List<Transaction>
            {
                new()
                {
                    UserId = command.UserId,
                    AccountId = command.AccountId,
                    CategoryId = command.CategoryId,
                    Description = command.Description,
                    Amount = command.Amount,
                    Type = command.Type,
                    Status = command.Status,
                    Date = command.Date,
                    DueDate = command.DueDate,
                    Notes = command.Notes
                }
            };

        TransactionDto? first = null;
        foreach (var tx in transactions)
        {
            var saved = await TransactionPostingService.PostAsync(tx, txRepo, accRepo, catRepo, invoiceRepo, command.UserId);
            first ??= saved.ToDto();
        }

        await cache.RemoveByPrefixAsync($"aurora:dashboard:{command.UserId}", ct);
        return first!;
    }
}
