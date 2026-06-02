using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class StudySkill : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public StudyCategory Category { get; set; } = StudyCategory.Other;
    public LifeArea Area { get; set; } = LifeArea.Studies;
    public StudySkillStatus Status { get; set; } = StudySkillStatus.Backlog;
    public int? PriorityRank { get; set; }
    public decimal PriorityScore { get; set; }
    public string? Purpose { get; set; }
    public string? CurrentLevel { get; set; }
    public string? TargetLevel { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? TargetDate { get; set; }
    public int WeeklyTimeBudgetMinutes { get; set; }

    public void ApplyPriorityScore(decimal score)
    {
        PriorityScore = score;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate(int rank)
    {
        Status = StudySkillStatus.Active;
        PriorityRank = rank;
        StartDate ??= DateTime.UtcNow.Date;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pause()
    {
        Status = StudySkillStatus.Paused;
        PriorityRank = null;
        UpdatedAt = DateTime.UtcNow;
    }
}

