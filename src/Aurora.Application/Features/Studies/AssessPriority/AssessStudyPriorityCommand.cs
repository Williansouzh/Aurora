using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Studies.AssessPriority;

public record AssessStudyPriorityCommand(
    string UserId,
    string SkillId,
    int Impact,
    int Urgency,
    int Alignment,
    int PrerequisitePower,
    int Motivation,
    int Applicability,
    int MaintenanceCost,
    string? Notes) : IRequest<StudyPriorityAssessmentDto>;

public class AssessStudyPriorityValidator : AbstractValidator<AssessStudyPriorityCommand>
{
    public AssessStudyPriorityValidator()
    {
        RuleFor(x => x.SkillId).NotEmpty();
        RuleFor(x => x.Impact).InclusiveBetween(1, 5);
        RuleFor(x => x.Urgency).InclusiveBetween(1, 5);
        RuleFor(x => x.Alignment).InclusiveBetween(1, 5);
        RuleFor(x => x.PrerequisitePower).InclusiveBetween(1, 5);
        RuleFor(x => x.Motivation).InclusiveBetween(1, 5);
        RuleFor(x => x.Applicability).InclusiveBetween(1, 5);
        RuleFor(x => x.MaintenanceCost).InclusiveBetween(1, 5);
    }
}

public class AssessStudyPriorityHandler(
    IStudySkillRepository skills,
    IStudyPriorityAssessmentRepository assessments)
    : IRequestHandler<AssessStudyPriorityCommand, StudyPriorityAssessmentDto>
{
    public async Task<StudyPriorityAssessmentDto> Handle(AssessStudyPriorityCommand cmd, CancellationToken ct)
    {
        var skill = await skills.GetByIdAsync(cmd.SkillId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Habilidade de estudo nao encontrada.");

        var score =
            cmd.Impact * 2m
            + cmd.Urgency * 1.5m
            + cmd.Alignment * 2m
            + cmd.PrerequisitePower * 1.5m
            + cmd.Motivation
            + cmd.Applicability * 1.5m
            - cmd.MaintenanceCost;

        var assessment = new StudyPriorityAssessment
        {
            UserId = cmd.UserId,
            SkillId = cmd.SkillId,
            Impact = cmd.Impact,
            Urgency = cmd.Urgency,
            Alignment = cmd.Alignment,
            PrerequisitePower = cmd.PrerequisitePower,
            Motivation = cmd.Motivation,
            Applicability = cmd.Applicability,
            MaintenanceCost = cmd.MaintenanceCost,
            Score = score,
            Notes = string.IsNullOrWhiteSpace(cmd.Notes) ? null : cmd.Notes.Trim()
        };

        skill.ApplyPriorityScore(score);
        await assessments.AddAsync(assessment, ct);
        await skills.UpdateAsync(skill, ct);

        return assessment.ToDto();
    }
}

