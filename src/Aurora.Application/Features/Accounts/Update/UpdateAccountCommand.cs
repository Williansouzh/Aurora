using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Accounts.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Accounts.Update;

public record UpdateAccountCommand(
    string UserId,
    string Id,
    string Name,
    AccountType Type,
    decimal InitialBalance,
    string Color,
    bool IsArchived,
    decimal CreditLimit = 0,
    int ClosingDay = 10,
    int DueDay = 15) : IRequest<AccountDto>;

public class UpdateAccountHandler(IAccountRepository accounts) : IRequestHandler<UpdateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(UpdateAccountCommand command, CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync(command.Id, command.UserId)
            ?? throw new NotFoundException("Conta nao encontrada");

        if (command.Type == AccountType.CreditCard)
        {
            if (command.CreditLimit <= 0)
            {
                throw new ValidationException("Limite do cartao deve ser positivo");
            }
            if (command.ClosingDay < 1 || command.ClosingDay > 28 || command.DueDay < 1 || command.DueDay > 28)
            {
                throw new ValidationException("Fechamento e vencimento devem estar entre 1 e 28");
            }
        }

        account.Name = command.Name;
        account.Type = command.Type;
        account.Color = command.Color;
        account.IsArchived = command.IsArchived;
        account.CreditLimit = command.Type == AccountType.CreditCard ? command.CreditLimit : 0;
        account.ClosingDay = command.ClosingDay;
        account.DueDay = command.DueDay;

        await accounts.UpdateAsync(account);
        return account.ToDto();
    }
}
