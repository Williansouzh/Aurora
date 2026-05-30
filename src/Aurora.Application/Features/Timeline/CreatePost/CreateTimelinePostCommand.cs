using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Timeline.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Timeline.CreatePost;

public record CreateTimelinePostCommand(
    string UserId,
    string Title,
    string? Description,
    LifeArea? Area,
    List<string>? MediaUrls) : IRequest<TimelineEventDto>;

public class CreateTimelinePostValidator : AbstractValidator<CreateTimelinePostCommand>
{
    public CreateTimelinePostValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

public class CreateTimelinePostHandler(ITimelineEventRepository repo)
    : IRequestHandler<CreateTimelinePostCommand, TimelineEventDto>
{
    public async Task<TimelineEventDto> Handle(CreateTimelinePostCommand cmd, CancellationToken ct)
    {
        var evt = new TimelineEvent
        {
            UserId = cmd.UserId,
            Type = TimelineEventType.ManualPost,
            Area = cmd.Area,
            Title = cmd.Title,
            Description = cmd.Description,
            OccurredAt = DateTime.UtcNow,
            SourceModule = "Manual",
            Visibility = TimelineVisibility.Private,
            MediaUrls = cmd.MediaUrls ?? [],
        };

        await repo.AddAsync(evt, ct);
        return evt.ToDto();
    }
}
