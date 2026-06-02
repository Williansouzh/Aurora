using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Studies.CreateSkill;

public record CreateStudySkillCommand(
    string UserId,
    string Title,
    StudyCategory Category,
    string? Purpose,
    string? CurrentLevel,
    string? TargetLevel,
    DateTime? TargetDate,
    int WeeklyTimeBudgetMinutes) : IRequest<StudySkillDto>;

public class CreateStudySkillValidator : AbstractValidator<CreateStudySkillCommand>
{
    public CreateStudySkillValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
        RuleFor(x => x.WeeklyTimeBudgetMinutes).InclusiveBetween(0, 3000);
    }
}

public class CreateStudySkillHandler(IStudySkillRepository skills)
    : IRequestHandler<CreateStudySkillCommand, StudySkillDto>
{
    public async Task<StudySkillDto> Handle(CreateStudySkillCommand cmd, CancellationToken ct)
    {
        var skill = new StudySkill
        {
            UserId = cmd.UserId,
            Title = cmd.Title.Trim(),
            Category = cmd.Category,
            Purpose = string.IsNullOrWhiteSpace(cmd.Purpose) ? null : cmd.Purpose.Trim(),
            CurrentLevel = string.IsNullOrWhiteSpace(cmd.CurrentLevel) ? null : cmd.CurrentLevel.Trim(),
            TargetLevel = string.IsNullOrWhiteSpace(cmd.TargetLevel) ? null : cmd.TargetLevel.Trim(),
            TargetDate = cmd.TargetDate,
            WeeklyTimeBudgetMinutes = cmd.WeeklyTimeBudgetMinutes,
            Status = StudySkillStatus.Backlog
        };

        await skills.AddAsync(skill, ct);
        return skill.ToDto();
    }
}

