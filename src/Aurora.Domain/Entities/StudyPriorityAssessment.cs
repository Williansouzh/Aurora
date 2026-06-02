using Aurora.Domain.Common;

namespace Aurora.Domain.Entities;

public class StudyPriorityAssessment : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string SkillId { get; set; } = string.Empty;
    public int Impact { get; set; }
    public int Urgency { get; set; }
    public int Alignment { get; set; }
    public int PrerequisitePower { get; set; }
    public int Motivation { get; set; }
    public int Applicability { get; set; }
    public int MaintenanceCost { get; set; }
    public decimal Score { get; set; }
    public string? Notes { get; set; }
}

