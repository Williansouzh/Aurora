using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Goals.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Goals.Update;

public record UpdateGoalCommand(
    string UserId,
    string Id,
    string Title,
    string? Description,
    LifeArea Area,
    DateTime? StartDate,
    DateTime? TargetDate,
    GoalMetricType MetricType,
    decimal TargetValue,
    string? CoverImage) : IRequest<GoalDto>;

public class UpdateGoalHandler(IGoalRepository repo) : IRequestHandler<UpdateGoalCommand, GoalDto>
{
    public async Task<GoalDto> Handle(UpdateGoalCommand cmd, CancellationToken ct)
    {
        var goal = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Meta não encontrada.");

        goal.Title = cmd.Title;
        goal.Description = cmd.Description;
        goal.Area = cmd.Area;
        goal.StartDate = cmd.StartDate;
        goal.TargetDate = cmd.TargetDate;
        goal.MetricType = cmd.MetricType;
        goal.TargetValue = cmd.TargetValue;
        goal.CoverImage = cmd.CoverImage;
        goal.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateAsync(goal, ct);
        return goal.ToDto();
    }
}
