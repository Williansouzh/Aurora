using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Studies.CreatePracticeTask;

public record CreateStudyPracticeTaskCommand(
    string UserId,
    string SkillId,
    string? TopicId,
    string Title,
    string? Instructions,
    DateTime DueDate,
    int Difficulty) : IRequest<StudyPracticeTaskDto>;

public class CreateStudyPracticeTaskValidator : AbstractValidator<CreateStudyPracticeTaskCommand>
{
    public CreateStudyPracticeTaskValidator()
    {
        RuleFor(x => x.SkillId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(180);
        RuleFor(x => x.Difficulty).InclusiveBetween(1, 5);
    }
}

public class CreateStudyPracticeTaskHandler(
    IStudyPracticeTaskRepository practices,
    IStudySkillRepository skills,
    IStudyTopicRepository topics,
    IDailyTaskRepository dailyTasks)
    : IRequestHandler<CreateStudyPracticeTaskCommand, StudyPracticeTaskDto>
{
    public async Task<StudyPracticeTaskDto> Handle(CreateStudyPracticeTaskCommand cmd, CancellationToken ct)
    {
        var skill = await skills.GetByIdAsync(cmd.SkillId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Habilidade de estudo nao encontrada.");

        if (!string.IsNullOrWhiteSpace(cmd.TopicId))
        {
            var topic = await topics.GetByIdAsync(cmd.TopicId, cmd.UserId, ct)
                ?? throw new InvalidOperationException("Topico de estudo nao encontrado.");
            if (topic.SkillId != skill.Id)
            {
                throw new InvalidOperationException("Topico nao pertence a habilidade informada.");
            }
        }

        var practice = new StudyPracticeTask
        {
            UserId = cmd.UserId,
            SkillId = skill.Id,
            TopicId = string.IsNullOrWhiteSpace(cmd.TopicId) ? null : cmd.TopicId,
            Title = cmd.Title.Trim(),
            Instructions = string.IsNullOrWhiteSpace(cmd.Instructions) ? null : cmd.Instructions.Trim(),
            DueDate = cmd.DueDate.Date,
            Difficulty = cmd.Difficulty
        };

        await practices.AddAsync(practice, ct);
        await dailyTasks.AddAsync(new DailyTask
        {
            UserId = cmd.UserId,
            Title = $"Praticar: {skill.Title}",
            Notes = practice.Title,
            Priority = practice.Difficulty >= 4 ? DailyTaskPriority.High : DailyTaskPriority.Medium,
            Date = practice.DueDate,
            Status = DailyTaskStatus.Pending,
            SourceModule = "Studies",
            SourceId = practice.Id
        }, ct);

        return practice.ToDto(skill.Title);
    }
}
