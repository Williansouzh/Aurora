using Aurora.Domain.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;

namespace Aurora.Domain.Entities;

public class StudyPracticeTask : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string SkillId { get; set; } = string.Empty;
    public string? TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public DateTime DueDate { get; set; }
    public StudyPracticeStatus Status { get; set; } = StudyPracticeStatus.Planned;
    public int Difficulty { get; set; } = 3;
    public int? ResultScore { get; set; }
    public string? SubmissionNotes { get; set; }
    public string? FeynmanExplanation { get; set; }
    public string? Mistakes { get; set; }
    public string? Doubts { get; set; }
    public string? NextAction { get; set; }
    public int XpGenerated { get; set; }
    public DateTime? CompletedAt { get; set; }

    public void Complete(
        int resultScore,
        string? submissionNotes,
        string? feynmanExplanation,
        string? mistakes,
        string? doubts,
        string? nextAction)
    {
        if (Status == StudyPracticeStatus.Completed)
        {
            throw new DomainException("Tarefa de pratica ja foi concluida.");
        }

        if (resultScore is < 1 or > 5)
        {
            throw new ValidationException("Resultado deve estar entre 1 e 5.");
        }

        ResultScore = resultScore;
        SubmissionNotes = submissionNotes;
        FeynmanExplanation = feynmanExplanation;
        Mistakes = mistakes;
        Doubts = doubts;
        NextAction = nextAction;
        Status = StudyPracticeStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        XpGenerated = CalculateXp(Difficulty, resultScore, feynmanExplanation, mistakes, doubts);
        UpdatedAt = DateTime.UtcNow;
    }

    public static int CalculateXp(int difficulty, int resultScore, string? feynmanExplanation, string? mistakes, string? doubts)
    {
        var baseXp = 12 + Math.Clamp(difficulty, 1, 5) * 4;
        var resultXp = resultScore * 3;
        var feynmanXp = string.IsNullOrWhiteSpace(feynmanExplanation) ? 0 : 12;
        var reflectionXp = string.IsNullOrWhiteSpace(mistakes) && string.IsNullOrWhiteSpace(doubts) ? 0 : 6;
        return baseXp + resultXp + feynmanXp + reflectionXp;
    }
}
