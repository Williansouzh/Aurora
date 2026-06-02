using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class ModuleCatalogItem : EntityBase
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LifeArea? Area { get; set; }
    public ModuleStatus Status { get; set; } = ModuleStatus.Enabled;
    public ModuleReleaseStage ReleaseStage { get; set; } = ModuleReleaseStage.Internal;
    public int SortOrder { get; set; }
    public string Icon { get; set; } = "box";
    public string Route { get; set; } = string.Empty;
    public UserRole RequiredRole { get; set; } = UserRole.User;
    public bool ShowInNavigation { get; set; } = true;
}
