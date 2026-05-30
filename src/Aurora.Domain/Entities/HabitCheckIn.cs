using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class HabitCheckIn : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string HabitId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public HabitCheckInStatus Status { get; set; } = HabitCheckInStatus.Done;
    public string? Note { get; set; }
    public string? PhotoUrl { get; set; }
    public int XpGenerated { get; set; }
}
