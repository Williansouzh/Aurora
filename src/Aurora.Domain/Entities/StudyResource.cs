using Aurora.Domain.Common;
using Aurora.Domain.Enums;

namespace Aurora.Domain.Entities;

public class StudyResource : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string SkillId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public StudyResourceType Type { get; set; } = StudyResourceType.Other;
    public string? Url { get; set; }
    public string? Author { get; set; }
    public int Reliability { get; set; } = 3;
    public StudyResourceStatus Status { get; set; } = StudyResourceStatus.Planned;
    public int SortOrder { get; set; }
    public string? Notes { get; set; }
}

