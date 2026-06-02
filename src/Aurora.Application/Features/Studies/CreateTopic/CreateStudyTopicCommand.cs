using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Studies.CreateTopic;

public record CreateStudyTopicCommand(
    string UserId,
    string SkillId,
    string Title,
    string? ParentTopicId,
    StudyStage Stage,
    int Importance,
    int Confidence,
    string? Notes) : IRequest<StudyTopicDto>;

public class CreateStudyTopicValidator : AbstractValidator<CreateStudyTopicCommand>
{
    public CreateStudyTopicValidator()
    {
        RuleFor(x => x.SkillId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Importance).InclusiveBetween(1, 5);
        RuleFor(x => x.Confidence).InclusiveBetween(1, 5);
    }
}

public class CreateStudyTopicHandler(IStudyTopicRepository topics, IStudySkillRepository skills)
    : IRequestHandler<CreateStudyTopicCommand, StudyTopicDto>
{
    public async Task<StudyTopicDto> Handle(CreateStudyTopicCommand cmd, CancellationToken ct)
    {
        var skill = await skills.GetByIdAsync(cmd.SkillId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Habilidade de estudo nao encontrada.");

        var topic = new StudyTopic
        {
            UserId = cmd.UserId,
            SkillId = cmd.SkillId,
            Title = cmd.Title.Trim(),
            ParentTopicId = string.IsNullOrWhiteSpace(cmd.ParentTopicId) ? null : cmd.ParentTopicId,
            Stage = cmd.Stage,
            Importance = cmd.Importance,
            Confidence = cmd.Confidence,
            Notes = string.IsNullOrWhiteSpace(cmd.Notes) ? null : cmd.Notes.Trim(),
            Status = StudyTopicStatus.NotStarted
        };

        await topics.AddAsync(topic, ct);
        return topic.ToDto(skill.Title);
    }
}

