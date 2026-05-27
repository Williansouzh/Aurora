using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class Category : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
    public string Color { get; set; } = "#94a3b8";
    public string Icon { get; set; } = "tag";
    public bool IsDefault { get; set; }
}
