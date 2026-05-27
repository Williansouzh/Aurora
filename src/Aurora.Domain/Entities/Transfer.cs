using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class Transfer : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string FromAccountId { get; set; } = string.Empty;
    public string ToAccountId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public TransferStatus Status { get; set; } = TransferStatus.Completed;
}
