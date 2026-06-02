using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class Plan : EntityBase
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PlanStatus Status { get; set; } = PlanStatus.Active;
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public List<string> ModuleKeys { get; set; } = [];
    public Dictionary<string, int> Limits { get; set; } = [];
}
