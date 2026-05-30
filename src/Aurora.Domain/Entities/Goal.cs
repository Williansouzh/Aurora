using Aurora.Domain.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;

namespace Aurora.Domain.Entities;

public class Goal : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LifeArea Area { get; set; } = LifeArea.Projects;
    public GoalStatus Status { get; set; } = GoalStatus.Active;
    public DateTime? StartDate { get; set; }
    public DateTime? TargetDate { get; set; }
    public GoalMetricType MetricType { get; set; } = GoalMetricType.None;
    public decimal TargetValue { get; set; }
    public decimal CurrentValue { get; set; }
    public string? CoverImage { get; set; }
    public string? LinkedCategoryId { get; set; }
    public List<Milestone> Milestones { get; set; } = [];

    public decimal Progress => MetricType switch
    {
        GoalMetricType.Percentage => Math.Min(CurrentValue, 100),
        GoalMetricType.Numeric when TargetValue > 0 => Math.Min(Math.Round(CurrentValue / TargetValue * 100, 1), 100),
        _ => AllRequiredMilestonesCompleted() ? 100 : RequiredMilestoneProgress(),
    };

    public void UpdateProgress(decimal currentValue)
    {
        if (MetricType == GoalMetricType.None)
            throw new DomainException("Esta meta não usa métrica numérica.");
        CurrentValue = currentValue;
        UpdatedAt = DateTime.UtcNow;
        TryAutoComplete();
    }

    public Milestone AddMilestone(string title, bool isRequired)
    {
        var milestone = new Milestone { Title = title, IsRequired = isRequired };
        Milestones.Add(milestone);
        UpdatedAt = DateTime.UtcNow;
        return milestone;
    }

    public void CompleteMilestone(string milestoneId)
    {
        var m = Milestones.FirstOrDefault(x => x.Id == milestoneId)
            ?? throw new NotFoundException("Milestone não encontrado.");
        m.Complete();
        UpdatedAt = DateTime.UtcNow;
        TryAutoComplete();
    }

    public void ReopenMilestone(string milestoneId)
    {
        var m = Milestones.FirstOrDefault(x => x.Id == milestoneId)
            ?? throw new NotFoundException("Milestone não encontrado.");
        m.Reopen();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ForceComplete(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ValidationException("Motivo obrigatório para conclusão forçada.");
        Status = GoalStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pause() { Status = GoalStatus.Paused; UpdatedAt = DateTime.UtcNow; }
    public void Resume() { Status = GoalStatus.Active; UpdatedAt = DateTime.UtcNow; }
    public void Cancel() { Status = GoalStatus.Cancelled; UpdatedAt = DateTime.UtcNow; }

    private void TryAutoComplete()
    {
        if (Status == GoalStatus.Completed) return;
        if (AllRequiredMilestonesCompleted() && (MetricType == GoalMetricType.None || CurrentValue >= TargetValue))
            Status = GoalStatus.Completed;
    }

    private bool AllRequiredMilestonesCompleted() =>
        Milestones.Where(m => m.IsRequired).All(m => m.IsCompleted);

    private decimal RequiredMilestoneProgress()
    {
        var required = Milestones.Count(m => m.IsRequired);
        if (required == 0) return 0;
        var done = Milestones.Count(m => m.IsRequired && m.IsCompleted);
        return Math.Round(done / (decimal)required * 100, 1);
    }
}
