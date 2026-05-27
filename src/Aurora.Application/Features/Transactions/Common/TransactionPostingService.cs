using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.CreditCardInvoices.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;

namespace Aurora.Application.Features.Transactions.Common;

public static class TransactionPostingService
{
    public static async Task<Transaction> PostAsync(
        Transaction tx,
        ITransactionRepository txRepo,
        IAccountRepository accRepo,
        ICategoryRepository catRepo,
        ICreditCardInvoiceRepository invoiceRepo,
        string userId)
    {
        var account = await accRepo.GetByIdAsync(tx.AccountId, userId)
            ?? throw new ValidationException("Conta invalida");

        if (!string.IsNullOrWhiteSpace(tx.CategoryId))
        {
            _ = await catRepo.GetByIdAsync(tx.CategoryId, userId)
                ?? throw new ValidationException("Categoria invalida");
        }

        if (account.Type == AccountType.CreditCard && tx.Type == TransactionType.Expense)
        {
            var period = CreditCardInvoiceCalculator.Period(tx.Date, account.ClosingDay);
            var invoice = await invoiceRepo.GetByPeriodAsync(account.Id, userId, period.Month, period.Year);
            if (invoice is null)
            {
                invoice = new CreditCardInvoice
                {
                    UserId = userId,
                    AccountId = account.Id,
                    Month = period.Month,
                    Year = period.Year,
                    DueDate = CreditCardInvoiceCalculator.DueDate(period.Month, period.Year, account.DueDay)
                };
                await invoiceRepo.AddAsync(invoice);
            }
            invoice.TotalAmount += tx.Amount;
            await invoiceRepo.UpdateAsync(invoice);

            tx.Status = TransactionStatus.Pending;
            tx.CreditCardInvoiceId = invoice.Id;
            account.CurrentBalance = await invoiceRepo.SumOpenByAccountAsync(account.Id, userId);
            await accRepo.UpdateAsync(account);
        }
        else if (tx.Status == TransactionStatus.Paid)
        {
            account.CurrentBalance += TransactionMapper.Impact(tx.Type, tx.Amount);
            await accRepo.UpdateAsync(account);
        }

        await txRepo.AddAsync(tx);
        return tx;
    }
}
