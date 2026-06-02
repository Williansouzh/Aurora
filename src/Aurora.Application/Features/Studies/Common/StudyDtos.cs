using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.Studies.Common;

public record StudySkillDto(
    string Id,
    string Title,
    StudyCategory Category,
    StudySkillStatus Status,
    int? PriorityRank,
    decimal PriorityScore,
    string? Purpose,
    string? CurrentLevel,
    string? TargetLevel,
    DateTime? StartDate,
    DateTime? TargetDate,
    int WeeklyTimeBudgetMinutes,
    DateTime CreatedAt);

public record StudyPriorityAssessmentDto(
    string Id,
    string SkillId,
    int Impact,
    int Urgency,
    int Alignment,
    int PrerequisitePower,
    int Motivation,
    int Applicability,
    int MaintenanceCost,
    decimal Score,
    string? Notes,
    DateTime CreatedAt);

public record StudySessionDto(
    string Id,
    string SkillId,
    string SkillTitle,
    DateTime Date,
    int PlannedMinutes,
    int? ActualMinutes,
    StudyStage Stage,
    StudySessionStatus Status,
    int? FocusScore,
    int? EnergyScore,
    int? DifficultyScore,
    string? Summary,
    string? FeynmanExplanation,
    string? NextAction,
    int XpGenerated,
    DateTime? CompletedAt,
    DateTime CreatedAt);

public record StudyReviewDto(
    string Id,
    string SkillId,
    string SkillTitle,
    string? SourceSessionId,
    string Title,
    string? Prompt,
    DateTime DueDate,
    StudyReviewStatus Status,
    DateTime? CompletedAt,
    StudyReviewResult? Result,
    int? ConfidenceBefore,
    int? ConfidenceAfter,
    DateTime? NextDueDate,
    int ReviewCount,
    DateTime CreatedAt);

public record StudyTopicDto(
    string Id,
    string SkillId,
    string SkillTitle,
    string Title,
    string? ParentTopicId,
    StudyStage Stage,
    StudyTopicStatus Status,
    int Importance,
    int Confidence,
    string? Notes,
    DateTime CreatedAt);

public record StudyResourceDto(
    string Id,
    string SkillId,
    string SkillTitle,
    string Title,
    StudyResourceType Type,
    string? Url,
    string? Author,
    int Reliability,
    StudyResourceStatus Status,
    int SortOrder,
    string? Notes,
    DateTime CreatedAt);

public record StudyPracticeTaskDto(
    string Id,
    string SkillId,
    string SkillTitle,
    string? TopicId,
    string Title,
    string? Instructions,
    DateTime DueDate,
    StudyPracticeStatus Status,
    int Difficulty,
    int? ResultScore,
    string? SubmissionNotes,
    string? FeynmanExplanation,
    string? Mistakes,
    string? Doubts,
    string? NextAction,
    int XpGenerated,
    DateTime? CompletedAt,
    DateTime CreatedAt);

public record StudyDashboardDto(
    int TotalSkills,
    int ActiveSkills,
    int BacklogSkills,
    int WeeklyTimeBudgetMinutes,
    int CompletedMinutesThisWeek,
    int DueReviews,
    int CompletedReviewsThisWeek,
    int OpenPracticeTasks,
    int CompletedPracticeTasksThisWeek,
    double AveragePracticeDifficultyThisWeek,
    List<StudySkillDto> ActivePriorities,
    List<StudySkillDto> RecommendedPriorities,
    List<StudySessionDto> RecentSessions,
    List<StudyReviewDto> DueReviewItems,
    List<StudyPracticeTaskDto> PracticeItems);

public static class StudyMappingExtensions
{
    public static StudySkillDto ToDto(this StudySkill skill) => new(
        skill.Id,
        skill.Title,
        skill.Category,
        skill.Status,
        skill.PriorityRank,
        skill.PriorityScore,
        skill.Purpose,
        skill.CurrentLevel,
        skill.TargetLevel,
        skill.StartDate,
        skill.TargetDate,
        skill.WeeklyTimeBudgetMinutes,
        skill.CreatedAt);

    public static StudyPriorityAssessmentDto ToDto(this StudyPriorityAssessment assessment) => new(
        assessment.Id,
        assessment.SkillId,
        assessment.Impact,
        assessment.Urgency,
        assessment.Alignment,
        assessment.PrerequisitePower,
        assessment.Motivation,
        assessment.Applicability,
        assessment.MaintenanceCost,
        assessment.Score,
        assessment.Notes,
        assessment.CreatedAt);

    public static StudySessionDto ToDto(this StudySession session, string skillTitle) => new(
        session.Id,
        session.SkillId,
        skillTitle,
        session.Date,
        session.PlannedMinutes,
        session.ActualMinutes,
        session.Stage,
        session.Status,
        session.FocusScore,
        session.EnergyScore,
        session.DifficultyScore,
        session.Summary,
        session.FeynmanExplanation,
        session.NextAction,
        session.XpGenerated,
        session.CompletedAt,
        session.CreatedAt);

    public static StudyReviewDto ToDto(this StudyReview review, string skillTitle) => new(
        review.Id,
        review.SkillId,
        skillTitle,
        review.SourceSessionId,
        review.Title,
        review.Prompt,
        review.DueDate,
        review.Status,
        review.CompletedAt,
        review.Result,
        review.ConfidenceBefore,
        review.ConfidenceAfter,
        review.NextDueDate,
        review.ReviewCount,
        review.CreatedAt);

    public static StudyTopicDto ToDto(this StudyTopic topic, string skillTitle) => new(
        topic.Id,
        topic.SkillId,
        skillTitle,
        topic.Title,
        topic.ParentTopicId,
        topic.Stage,
        topic.Status,
        topic.Importance,
        topic.Confidence,
        topic.Notes,
        topic.CreatedAt);

    public static StudyResourceDto ToDto(this StudyResource resource, string skillTitle) => new(
        resource.Id,
        resource.SkillId,
        skillTitle,
        resource.Title,
        resource.Type,
        resource.Url,
        resource.Author,
        resource.Reliability,
        resource.Status,
        resource.SortOrder,
        resource.Notes,
        resource.CreatedAt);

    public static StudyPracticeTaskDto ToDto(this StudyPracticeTask practice, string skillTitle) => new(
        practice.Id,
        practice.SkillId,
        skillTitle,
        practice.TopicId,
        practice.Title,
        practice.Instructions,
        practice.DueDate,
        practice.Status,
        practice.Difficulty,
        practice.ResultScore,
        practice.SubmissionNotes,
        practice.FeynmanExplanation,
        practice.Mistakes,
        practice.Doubts,
        practice.NextAction,
        practice.XpGenerated,
        practice.CompletedAt,
        practice.CreatedAt);
}
