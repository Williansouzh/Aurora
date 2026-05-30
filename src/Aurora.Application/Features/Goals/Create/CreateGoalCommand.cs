using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Goals.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Goals.Create;

public record CreateGoalCommand(
    string UserId,
    string Title,
    string? Description,
    LifeArea Area,
    DateTime? StartDate,
    DateTime? TargetDate,
    GoalMetricType MetricType,
    decimal TargetValue,
    string? CoverImage,
    string? LinkedCategoryId = null) : IRequest<GoalDto>;

public class CreateGoalValidator : AbstractValidator<CreateGoalCommand>
{
    public CreateGoalValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.TargetValue).GreaterThanOrEqualTo(0);
    }
}

public class CreateGoalHandler(IGoalRepository repo) : IRequestHandler<CreateGoalCommand, GoalDto>
{
    public async Task<GoalDto> Handle(CreateGoalCommand cmd, CancellationToken ct)
    {
        var goal = new Goal
        {
            UserId = cmd.UserId,
            Title = cmd.Title,
            Description = cmd.Description,
            Area = cmd.Area,
            StartDate = cmd.StartDate,
            TargetDate = cmd.TargetDate,
            MetricType = cmd.MetricType,
            TargetValue = cmd.TargetValue,
            CoverImage = cmd.CoverImage,
            LinkedCategoryId = cmd.LinkedCategoryId,
        };

        await repo.AddAsync(goal, ct);
        return goal.ToDto();
    }
}
