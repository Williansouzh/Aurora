using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class EvolutionAlbum : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public LifeArea Area { get; set; } = LifeArea.Health;
    public string? Description { get; set; }
    public string? CoverImage { get; set; }
    public bool IsPrivate { get; set; } = true;
}
