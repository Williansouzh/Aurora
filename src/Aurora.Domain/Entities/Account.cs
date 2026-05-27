using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class Account : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal InitialBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Color { get; set; } = "#6366f1";
    public bool IsArchived { get; set; }
    public decimal CreditLimit { get; set; }
    public int ClosingDay { get; set; } = 10;
    public int DueDay { get; set; } = 15;
}
