using Aurora.Domain.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;

namespace Aurora.Domain.Entities;

public class StudySession : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string SkillId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int PlannedMinutes { get; set; }
    public int? ActualMinutes { get; set; }
    public StudyStage Stage { get; set; } = StudyStage.Obtain;
    public StudySessionStatus Status { get; set; } = StudySessionStatus.Planned;
    public int? FocusScore { get; set; }
    public int? EnergyScore { get; set; }
    public int? DifficultyScore { get; set; }
    public string? Summary { get; set; }
    public string? FeynmanExplanation { get; set; }
    public string? NextAction { get; set; }
    public int XpGenerated { get; set; }
    public DateTime? CompletedAt { get; set; }

    public void Finish(
        int actualMinutes,
        int focusScore,
        int energyScore,
        int difficultyScore,
        string? summary,
        string? feynmanExplanation,
        string? nextAction)
    {
        if (Status == StudySessionStatus.Completed)
        {
            throw new DomainException("Sessao de estudo ja foi finalizada.");
        }

        if (actualMinutes <= 0)
        {
            throw new ValidationException("Tempo real deve ser maior que zero.");
        }

        ActualMinutes = actualMinutes;
        FocusScore = focusScore;
        EnergyScore = energyScore;
        DifficultyScore = difficultyScore;
        Summary = summary;
        FeynmanExplanation = feynmanExplanation;
        NextAction = nextAction;
        Status = StudySessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        XpGenerated = CalculateXp(actualMinutes, Stage, feynmanExplanation);
        UpdatedAt = DateTime.UtcNow;
    }

    public static int CalculateXp(int actualMinutes, StudyStage stage, string? feynmanExplanation)
    {
        var timeXp = Math.Min(30, Math.Max(5, actualMinutes / 5));
        var stageXp = stage switch
        {
            StudyStage.Apply => 15,
            StudyStage.Teach => 20,
            StudyStage.Memorize => 10,
            StudyStage.Organize => 8,
            _ => 5
        };
        var feynmanXp = string.IsNullOrWhiteSpace(feynmanExplanation) ? 0 : 10;
        return timeXp + stageXp + feynmanXp;
    }
}

