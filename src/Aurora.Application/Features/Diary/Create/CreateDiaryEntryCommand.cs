using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Diary.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Diary.Create;

public record CreateDiaryEntryCommand(
    string UserId,
    DateTime Date,
    string Content,
    int Mood,
    List<string>? Tags) : IRequest<DiaryEntryDto>;

public class CreateDiaryEntryValidator : AbstractValidator<CreateDiaryEntryCommand>
{
    public CreateDiaryEntryValidator()
    {
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.Mood).InclusiveBetween(1, 5);
    }
}

public class CreateDiaryEntryHandler(IDiaryEntryRepository repo, ITimelineEventRepository timelineRepo)
    : IRequestHandler<CreateDiaryEntryCommand, DiaryEntryDto>
{
    public async Task<DiaryEntryDto> Handle(CreateDiaryEntryCommand cmd, CancellationToken ct)
    {
        var date = cmd.Date.Date;
        if (await repo.GetByDateAsync(cmd.UserId, date) is not null)
            throw new ConflictException("Já existe um registro no diário para esta data.");

        var entry = new DiaryEntry
        {
            UserId = cmd.UserId,
            Date = date,
            Content = cmd.Content,
            Mood = cmd.Mood,
            Tags = cmd.Tags ?? [],
        };

        await repo.AddAsync(entry, ct);

        await timelineRepo.AddFromModuleAsync(new TimelineEvent
        {
            UserId = cmd.UserId,
            Type = TimelineEventType.DiaryWritten,
            Title = $"Diário — {date:dd/MM/yyyy}",
            OccurredAt = entry.CreatedAt,
            SourceModule = "Diary",
            SourceId = entry.Id,
            Visibility = TimelineVisibility.Private,
        });

        return entry.ToDto();
    }
}
