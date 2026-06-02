using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class LifeAreaCatalogItem : EntityBase
{
    public string Key { get; set; } = string.Empty;
    public LifeArea Area { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#64748b";
    public string Icon { get; set; } = "circle";
    public ModuleStatus Status { get; set; } = ModuleStatus.Enabled;
    public int SortOrder { get; set; }
}
