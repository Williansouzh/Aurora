using Aurora.Domain.Common;

namespace Aurora.Domain.Entities;

public class EvolutionPhoto : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string AlbumId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public DateTime Date { get; set; }
    public List<string> Tags { get; set; } = [];
    public string? LinkedGoalId { get; set; }
    public string? LinkedHabitId { get; set; }
}
