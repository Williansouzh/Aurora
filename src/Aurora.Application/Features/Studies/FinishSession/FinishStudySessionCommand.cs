using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Studies.FinishSession;

public record FinishStudySessionCommand(
    string UserId,
    string SessionId,
    int ActualMinutes,
    int FocusScore,
    int EnergyScore,
    int DifficultyScore,
    string? Summary,
    string? FeynmanExplanation,
    string? NextAction) : IRequest<StudySessionDto>;

public class FinishStudySessionValidator : AbstractValidator<FinishStudySessionCommand>
{
    public FinishStudySessionValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.ActualMinutes).InclusiveBetween(1, 720);
        RuleFor(x => x.FocusScore).InclusiveBetween(1, 5);
        RuleFor(x => x.EnergyScore).InclusiveBetween(1, 5);
        RuleFor(x => x.DifficultyScore).InclusiveBetween(1, 5);
    }
}

public class FinishStudySessionHandler(
    IStudySessionRepository sessions,
    IStudySkillRepository skills,
    IStudyReviewRepository reviews,
    IDailyTaskRepository dailyTasks,
    ITimelineEventRepository timeline,
    IXpService xp)
    : IRequestHandler<FinishStudySessionCommand, StudySessionDto>
{
    public async Task<StudySessionDto> Handle(FinishStudySessionCommand cmd, CancellationToken ct)
    {
        var session = await sessions.GetByIdAsync(cmd.SessionId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Sessao de estudo nao encontrada.");
        var skill = await skills.GetByIdAsync(session.SkillId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Habilidade de estudo nao encontrada.");

        session.Finish(
            cmd.ActualMinutes,
            cmd.FocusScore,
            cmd.EnergyScore,
            cmd.DifficultyScore,
            string.IsNullOrWhiteSpace(cmd.Summary) ? session.Summary : cmd.Summary.Trim(),
            string.IsNullOrWhiteSpace(cmd.FeynmanExplanation) ? null : cmd.FeynmanExplanation.Trim(),
            string.IsNullOrWhiteSpace(cmd.NextAction) ? null : cmd.NextAction.Trim());

        await sessions.UpdateAsync(session, ct);
        var linkedTask = await dailyTasks.GetBySourceAsync(cmd.UserId, "Studies", session.Id, ct);
        if (linkedTask is not null && linkedTask.Status != DailyTaskStatus.Completed)
        {
            linkedTask.Complete();
            await dailyTasks.UpdateAsync(linkedTask, ct);
        }

        await reviews.AddAsync(new StudyReview
        {
            UserId = cmd.UserId,
            SkillId = skill.Id,
            SourceSessionId = session.Id,
            Title = $"Revisar sessao: {skill.Title}",
            Prompt = session.FeynmanExplanation ?? session.Summary ?? "Explique o principal conceito estudado sem consultar as notas.",
            DueDate = DateTime.UtcNow.Date.AddDays(1),
            ReviewCount = 0
        }, ct);

        await xp.AwardAsync(cmd.UserId, XpSource.StudySession, session.XpGenerated, $"Sessao de estudo: {skill.Title}", ct);
        await timeline.AddFromModuleAsync(new TimelineEvent
        {
            UserId = cmd.UserId,
            Type = TimelineEventType.StudySessionCompleted,
            Area = LifeArea.Studies,
            Title = $"Sessao de estudos concluida: {skill.Title}",
            Description = $"{session.ActualMinutes} min em {StageName(session.Stage)}. +{session.XpGenerated} XP.",
            OccurredAt = DateTime.UtcNow,
            SourceModule = "Studies",
            SourceId = session.Id,
            Visibility = TimelineVisibility.Private
        });

        return session.ToDto(skill.Title);
    }

    private static string StageName(StudyStage stage) => stage switch
    {
        StudyStage.Obtain => "obter informacoes",
        StudyStage.Organize => "organizar",
        StudyStage.Memorize => "memorizar",
        StudyStage.Apply => "aplicar",
        StudyStage.Teach => "ensinar",
        _ => "estudar"
    };
}
