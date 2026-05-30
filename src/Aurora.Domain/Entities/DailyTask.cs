using Aurora.Domain.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;

namespace Aurora.Domain.Entities;

public class DailyTask : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DailyTaskPriority Priority { get; set; } = DailyTaskPriority.Medium;
    public DailyTaskStatus Status { get; set; } = DailyTaskStatus.Pending;
    public DateTime Date { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsBacklog { get; set; }

    public void Complete()
    {
        if (Status == DailyTaskStatus.Completed)
            throw new DomainException("Tarefa já foi concluída.");

        Status = DailyTaskStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        if (Status != DailyTaskStatus.Completed)
            throw new DomainException("Só é possível reabrir uma tarefa concluída.");

        Status = DailyTaskStatus.Pending;
        CompletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
