using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Accounts.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;
using ValidationException = Aurora.Domain.Exceptions.ValidationException;

namespace Aurora.Application.Features.Accounts.Create;

public record CreateAccountCommand(
    string UserId,
    string Name,
    AccountType Type,
    decimal InitialBalance,
    string Color,
    decimal CreditLimit = 0,
    int ClosingDay = 10,
    int DueDay = 15) : IRequest<AccountDto>;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.InitialBalance).GreaterThanOrEqualTo(0);
    }
}

public class CreateAccountHandler(IAccountRepository accounts) : IRequestHandler<CreateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(CreateAccountCommand command, CancellationToken ct)
    {
        ValidateCreditCard(command.Type, command.CreditLimit, command.ClosingDay, command.DueDay);

        var current = command.Type == AccountType.CreditCard ? 0 : command.InitialBalance;

        var account = new Account
        {
            UserId = command.UserId,
            Name = command.Name,
            Type = command.Type,
            InitialBalance = command.InitialBalance,
            CurrentBalance = current,
            Color = command.Color,
            CreditLimit = command.Type == AccountType.CreditCard ? command.CreditLimit : 0,
            ClosingDay = command.ClosingDay,
            DueDay = command.DueDay
        };

        await accounts.AddAsync(account);
        return account.ToDto();
    }

    private static void ValidateCreditCard(AccountType type, decimal limit, int closingDay, int dueDay)
    {
        if (type != AccountType.CreditCard) return;
        if (limit <= 0) throw new ValidationException("Limite do cartao deve ser positivo");
        if (closingDay < 1 || closingDay > 28 || dueDay < 1 || dueDay > 28)
        {
            throw new ValidationException("Fechamento e vencimento devem estar entre 1 e 28");
        }
    }
}
