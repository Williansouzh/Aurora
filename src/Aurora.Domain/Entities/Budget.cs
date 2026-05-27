using Aurora.Domain.Common;

namespace Aurora.Domain.Entities;

public class Budget : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal LimitAmount { get; set; }
}
