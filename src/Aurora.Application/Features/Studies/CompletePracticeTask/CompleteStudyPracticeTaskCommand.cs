using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Studies.CompletePracticeTask;

public record CompleteStudyPracticeTaskCommand(
    string UserId,
    string PracticeTaskId,
    int ResultScore,
    string? SubmissionNotes,
    string? FeynmanExplanation,
    string? Mistakes,
    string? Doubts,
    string? NextAction) : IRequest<StudyPracticeTaskDto>;

public class CompleteStudyPracticeTaskValidator : AbstractValidator<CompleteStudyPracticeTaskCommand>
{
    public CompleteStudyPracticeTaskValidator()
    {
        RuleFor(x => x.PracticeTaskId).NotEmpty();
        RuleFor(x => x.ResultScore).InclusiveBetween(1, 5);
        RuleFor(x => x.FeynmanExplanation).MaximumLength(4000);
        RuleFor(x => x.Mistakes).MaximumLength(3000);
        RuleFor(x => x.Doubts).MaximumLength(3000);
    }
}

public class CompleteStudyPracticeTaskHandler(
    IStudyPracticeTaskRepository practices,
    IStudySkillRepository skills,
    IStudyReviewRepository reviews,
    IDailyTaskRepository dailyTasks,
    ITimelineEventRepository timeline,
    IXpService xp)
    : IRequestHandler<CompleteStudyPracticeTaskCommand, StudyPracticeTaskDto>
{
    public async Task<StudyPracticeTaskDto> Handle(CompleteStudyPracticeTaskCommand cmd, CancellationToken ct)
    {
        var practice = await practices.GetByIdAsync(cmd.PracticeTaskId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Tarefa de pratica nao encontrada.");
        var skill = await skills.GetByIdAsync(practice.SkillId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Habilidade de estudo nao encontrada.");

        practice.Complete(
            cmd.ResultScore,
            Normalize(cmd.SubmissionNotes),
            Normalize(cmd.FeynmanExplanation),
            Normalize(cmd.Mistakes),
            Normalize(cmd.Doubts),
            Normalize(cmd.NextAction));

        await practices.UpdateAsync(practice, ct);
        var linkedTask = await dailyTasks.GetBySourceAsync(cmd.UserId, "Studies", practice.Id, ct);
        if (linkedTask is not null && linkedTask.Status != DailyTaskStatus.Completed)
        {
            linkedTask.Complete();
            await dailyTasks.UpdateAsync(linkedTask, ct);
        }

        await reviews.AddAsync(new StudyReview
        {
            UserId = cmd.UserId,
            SkillId = skill.Id,
            SourceSessionId = practice.Id,
            Title = $"Revisar pratica: {practice.Title}",
            Prompt = practice.FeynmanExplanation ?? practice.Mistakes ?? practice.Doubts ?? "Explique como voce resolveria novamente essa pratica sem consultar as notas.",
            DueDate = DateTime.UtcNow.Date.AddDays(practice.ResultScore <= 2 ? 1 : 3),
            ReviewCount = 0
        }, ct);

        await xp.AwardAsync(cmd.UserId, XpSource.StudyPractice, practice.XpGenerated, $"Pratica concluida: {skill.Title}", ct);
        await timeline.AddFromModuleAsync(new TimelineEvent
        {
            UserId = cmd.UserId,
            Type = TimelineEventType.StudyPracticeCompleted,
            Area = LifeArea.Studies,
            Title = $"Pratica concluida: {skill.Title}",
            Description = $"{practice.Title}. Resultado {practice.ResultScore}/5. +{practice.XpGenerated} XP.",
            OccurredAt = DateTime.UtcNow,
            SourceModule = "Studies",
            SourceId = practice.Id,
            Visibility = TimelineVisibility.Private
        });

        return practice.ToDto(skill.Title);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
