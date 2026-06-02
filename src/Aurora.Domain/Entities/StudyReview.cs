using Aurora.Domain.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;

namespace Aurora.Domain.Entities;

public class StudyReview : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string SkillId { get; set; } = string.Empty;
    public string? SourceSessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Prompt { get; set; }
    public DateTime DueDate { get; set; }
    public StudyReviewStatus Status { get; set; } = StudyReviewStatus.Pending;
    public DateTime? CompletedAt { get; set; }
    public StudyReviewResult? Result { get; set; }
    public int? ConfidenceBefore { get; set; }
    public int? ConfidenceAfter { get; set; }
    public DateTime? NextDueDate { get; set; }
    public int ReviewCount { get; set; }

    public void Complete(StudyReviewResult result, int confidenceBefore, int confidenceAfter)
    {
        if (Status == StudyReviewStatus.Completed)
        {
            throw new DomainException("Revisao ja foi concluida.");
        }

        if (confidenceBefore is < 1 or > 5 || confidenceAfter is < 1 or > 5)
        {
            throw new ValidationException("Confianca deve ser entre 1 e 5.");
        }

        Result = result;
        ConfidenceBefore = confidenceBefore;
        ConfidenceAfter = confidenceAfter;
        CompletedAt = DateTime.UtcNow;
        Status = StudyReviewStatus.Completed;
        NextDueDate = CalculateNextDueDate(DateTime.UtcNow.Date, result, ReviewCount);
        UpdatedAt = DateTime.UtcNow;
    }

    public StudyReview CreateNext()
    {
        if (NextDueDate is null || Result == StudyReviewResult.Again)
        {
            throw new DomainException("Esta revisao nao possui proximo agendamento automatico.");
        }

        return new StudyReview
        {
            UserId = UserId,
            SkillId = SkillId,
            SourceSessionId = SourceSessionId,
            Title = Title,
            Prompt = Prompt,
            DueDate = NextDueDate.Value,
            ReviewCount = ReviewCount + 1
        };
    }

    public static DateTime? CalculateNextDueDate(DateTime from, StudyReviewResult result, int reviewCount) => result switch
    {
        StudyReviewResult.Again => from.AddDays(1),
        StudyReviewResult.Hard => from.AddDays(reviewCount <= 0 ? 3 : 7),
        StudyReviewResult.Good => from.AddDays(reviewCount <= 0 ? 7 : reviewCount == 1 ? 14 : 30),
        StudyReviewResult.Easy => from.AddDays(reviewCount <= 0 ? 14 : 30),
        _ => null
    };
}

