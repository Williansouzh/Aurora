using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.WeeklyPlanning.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.WeeklyPlanning.Create;

public record CreateWeeklyPlanCommand(
    string UserId,
    DateTime WeekStart,
    string? MainFocus,
    List<string>? LinkedGoalIds,
    List<string>? Priorities,
    string? Notes) : IRequest<WeeklyPlanDto>;

public class CreateWeeklyPlanValidator : AbstractValidator<CreateWeeklyPlanCommand>
{
    public CreateWeeklyPlanValidator()
    {
        RuleFor(x => x.WeekStart).NotEmpty();
    }
}

public class CreateWeeklyPlanHandler(IWeeklyPlanRepository repo)
    : IRequestHandler<CreateWeeklyPlanCommand, WeeklyPlanDto>
{
    public async Task<WeeklyPlanDto> Handle(CreateWeeklyPlanCommand cmd, CancellationToken ct)
    {
        var weekStart = cmd.WeekStart.Date;
        if (await repo.ExistsForWeekAsync(cmd.UserId, weekStart))
            throw new ConflictException("Já existe um plano para esta semana.");

        var plan = new WeeklyPlan
        {
            UserId = cmd.UserId,
            WeekStart = weekStart,
            WeekEnd = weekStart.AddDays(6),
            MainFocus = cmd.MainFocus,
            LinkedGoalIds = cmd.LinkedGoalIds ?? [],
            Priorities = cmd.Priorities ?? [],
            Notes = cmd.Notes,
        };

        await repo.AddAsync(plan, ct);
        return plan.ToDto();
    }
}
