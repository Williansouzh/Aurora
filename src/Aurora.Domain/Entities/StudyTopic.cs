using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class StudyTopic : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string SkillId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ParentTopicId { get; set; }
    public StudyStage Stage { get; set; } = StudyStage.Obtain;
    public StudyTopicStatus Status { get; set; } = StudyTopicStatus.NotStarted;
    public int Importance { get; set; } = 3;
    public int Confidence { get; set; } = 1;
    public string? Notes { get; set; }
}

