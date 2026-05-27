using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Financings.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Financings.MarkInstallmentAsPaid;

public record MarkFinancingInstallmentAsPaidCommand(
    string UserId,
    string FinancingId,
    int Number,
    decimal? PaidAmount = null,
    DateTime? PaidAt = null,
    string? LinkedTransactionId = null) : IRequest<FinancingDto>;

public class MarkFinancingInstallmentAsPaidHandler(IFinancingRepository financings)
    : IRequestHandler<MarkFinancingInstallmentAsPaidCommand, FinancingDto>
{
    public async Task<FinancingDto> Handle(MarkFinancingInstallmentAsPaidCommand command, CancellationToken ct)
    {
        var f = await financings.GetByIdAsync(command.FinancingId, command.UserId)
            ?? throw new NotFoundException("Financiamento nao encontrado");

        var installment = f.Installments.FirstOrDefault(x => x.Number == command.Number)
            ?? throw new NotFoundException("Parcela nao encontrada");

        installment.Status = FinancingInstallmentStatus.Paid;
        installment.PaidAt = command.PaidAt ?? DateTime.UtcNow;
        installment.PaidAmount = command.PaidAmount ?? installment.TotalPayment;
        if (command.LinkedTransactionId != null)
        {
            installment.LinkedTransactionId = command.LinkedTransactionId;
        }

        if (f.Installments.All(x => x.Status == FinancingInstallmentStatus.Paid))
        {
            f.Status = FinancingStatus.PaidOff;
        }

        await financings.UpdateAsync(f);
        return f.ToDto();
    }
}
