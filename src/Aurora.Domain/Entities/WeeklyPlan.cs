using Aurora.Domain.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;

namespace Aurora.Domain.Entities;

public class WeeklyPlan : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public string? MainFocus { get; set; }
    public List<string> LinkedGoalIds { get; set; } = [];
    public List<string> Priorities { get; set; } = [];
    public string? Notes { get; set; }
    public WeeklyPlanStatus Status { get; set; } = WeeklyPlanStatus.NotStarted;
    public string? Review { get; set; }
    public int XpGenerated { get; set; }
    public DateTime? ClosedAt { get; set; }

    public void Start()
    {
        if (Status != WeeklyPlanStatus.NotStarted)
            throw new DomainException("Plano já foi iniciado.");
        Status = WeeklyPlanStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close(string? review)
    {
        if (Status == WeeklyPlanStatus.Closed)
            throw new DomainException("Plano já está encerrado.");
        Status = WeeklyPlanStatus.Closed;
        Review = review;
        ClosedAt = DateTime.UtcNow;
        XpGenerated = 30;
        UpdatedAt = DateTime.UtcNow;
    }
}
