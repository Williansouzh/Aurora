using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.Today.Common;

public record DailyTaskDto(
    string Id,
    string Title,
    string? Notes,
    DailyTaskPriority Priority,
    DailyTaskStatus Status,
    DateTime Date,
    DateTime? CompletedAt,
    string? SourceModule,
    string? SourceId,
    DateTime CreatedAt);

public static class DailyTaskMappingExtensions
{
    public static DailyTaskDto ToDto(this DailyTask t) => new(
        t.Id, t.Title, t.Notes, t.Priority, t.Status, t.Date, t.CompletedAt, t.SourceModule, t.SourceId, t.CreatedAt);
}
