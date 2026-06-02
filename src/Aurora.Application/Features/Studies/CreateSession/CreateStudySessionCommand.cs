using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Studies.CreateSession;

public record CreateStudySessionCommand(
    string UserId,
    string SkillId,
    DateTime Date,
    int PlannedMinutes,
    StudyStage Stage,
    string? Summary) : IRequest<StudySessionDto>;

public class CreateStudySessionValidator : AbstractValidator<CreateStudySessionCommand>
{
    public CreateStudySessionValidator()
    {
        RuleFor(x => x.SkillId).NotEmpty();
        RuleFor(x => x.PlannedMinutes).InclusiveBetween(5, 480);
    }
}

public class CreateStudySessionHandler(
    IStudySessionRepository sessions,
    IStudySkillRepository skills,
    IDailyTaskRepository dailyTasks)
    : IRequestHandler<CreateStudySessionCommand, StudySessionDto>
{
    public async Task<StudySessionDto> Handle(CreateStudySessionCommand cmd, CancellationToken ct)
    {
        var skill = await skills.GetByIdAsync(cmd.SkillId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Habilidade de estudo nao encontrada.");

        var session = new StudySession
        {
            UserId = cmd.UserId,
            SkillId = cmd.SkillId,
            Date = cmd.Date.Date,
            PlannedMinutes = cmd.PlannedMinutes,
            Stage = cmd.Stage,
            Summary = string.IsNullOrWhiteSpace(cmd.Summary) ? null : cmd.Summary.Trim(),
            Status = StudySessionStatus.Planned
        };

        await sessions.AddAsync(session, ct);
        await dailyTasks.AddAsync(new DailyTask
        {
            UserId = cmd.UserId,
            Title = $"Estudar: {skill.Title}",
            Notes = session.Summary ?? $"{session.PlannedMinutes} min em {StageName(session.Stage)}.",
            Priority = DailyTaskPriority.High,
            Date = session.Date,
            Status = DailyTaskStatus.Pending,
            SourceModule = "Studies",
            SourceId = session.Id
        }, ct);

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
