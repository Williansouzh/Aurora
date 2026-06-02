using Aurora.Application.Abstractions.Common;
using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Studies.CompleteReview;

public record CompleteStudyReviewCommand(
    string UserId,
    string ReviewId,
    StudyReviewResult Result,
    int ConfidenceBefore,
    int ConfidenceAfter) : IRequest<StudyReviewDto>;

public class CompleteStudyReviewValidator : AbstractValidator<CompleteStudyReviewCommand>
{
    public CompleteStudyReviewValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.ConfidenceBefore).InclusiveBetween(1, 5);
        RuleFor(x => x.ConfidenceAfter).InclusiveBetween(1, 5);
    }
}

public class CompleteStudyReviewHandler(
    IStudyReviewRepository reviews,
    IStudySkillRepository skills,
    ITimelineEventRepository timeline,
    IXpService xp)
    : IRequestHandler<CompleteStudyReviewCommand, StudyReviewDto>
{
    public async Task<StudyReviewDto> Handle(CompleteStudyReviewCommand cmd, CancellationToken ct)
    {
        var review = await reviews.GetByIdAsync(cmd.ReviewId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Revisao nao encontrada.");
        var skill = await skills.GetByIdAsync(review.SkillId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Habilidade de estudo nao encontrada.");

        review.Complete(cmd.Result, cmd.ConfidenceBefore, cmd.ConfidenceAfter);
        await reviews.UpdateAsync(review, ct);

        if (review.NextDueDate is not null)
        {
            await reviews.AddAsync(review.CreateNext(), ct);
        }

        var xpAmount = cmd.Result switch
        {
            StudyReviewResult.Again => 5,
            StudyReviewResult.Hard => 8,
            StudyReviewResult.Good => 10,
            StudyReviewResult.Easy => 12,
            _ => 8
        };

        await xp.AwardAsync(cmd.UserId, XpSource.StudyReview, xpAmount, $"Revisao de estudos: {skill.Title}", ct);
        await timeline.AddFromModuleAsync(new TimelineEvent
        {
            UserId = cmd.UserId,
            Type = TimelineEventType.StudyReviewCompleted,
            Area = LifeArea.Studies,
            Title = $"Revisao concluida: {skill.Title}",
            Description = $"{review.Title}. Resultado: {cmd.Result}. +{xpAmount} XP.",
            OccurredAt = DateTime.UtcNow,
            SourceModule = "Studies",
            SourceId = review.Id,
            Visibility = TimelineVisibility.Private
        });

        return review.ToDto(skill.Title);
    }
}
