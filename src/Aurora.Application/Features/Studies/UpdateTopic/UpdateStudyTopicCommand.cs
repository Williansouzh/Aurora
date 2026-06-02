using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Studies.UpdateTopic;

public record UpdateStudyTopicCommand(
    string UserId,
    string TopicId,
    string Title,
    StudyStage Stage,
    StudyTopicStatus Status,
    int Importance,
    int Confidence,
    string? Notes) : IRequest<StudyTopicDto>;

public class UpdateStudyTopicValidator : AbstractValidator<UpdateStudyTopicCommand>
{
    public UpdateStudyTopicValidator()
    {
        RuleFor(x => x.TopicId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Importance).InclusiveBetween(1, 5);
        RuleFor(x => x.Confidence).InclusiveBetween(1, 5);
    }
}

public class UpdateStudyTopicHandler(IStudyTopicRepository topics, IStudySkillRepository skills)
    : IRequestHandler<UpdateStudyTopicCommand, StudyTopicDto>
{
    public async Task<StudyTopicDto> Handle(UpdateStudyTopicCommand cmd, CancellationToken ct)
    {
        var topic = await topics.GetByIdAsync(cmd.TopicId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Topico de estudo nao encontrado.");
        var skill = await skills.GetByIdAsync(topic.SkillId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Habilidade de estudo nao encontrada.");

        topic.Title = cmd.Title.Trim();
        topic.Stage = cmd.Stage;
        topic.Status = cmd.Status;
        topic.Importance = cmd.Importance;
        topic.Confidence = cmd.Confidence;
        topic.Notes = string.IsNullOrWhiteSpace(cmd.Notes) ? null : cmd.Notes.Trim();
        await topics.UpdateAsync(topic, ct);
        return topic.ToDto(skill.Title);
    }
}

