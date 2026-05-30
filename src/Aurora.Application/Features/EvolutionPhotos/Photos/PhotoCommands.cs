using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.EvolutionPhotos.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.EvolutionPhotos.Photos;

public record AddPhotoCommand(
    string UserId,
    string AlbumId,
    string ImageUrl,
    string? Caption,
    DateTime Date,
    List<string>? Tags,
    string? LinkedGoalId,
    string? LinkedHabitId) : IRequest<EvolutionPhotoDto>;

public record DeletePhotoCommand(string UserId, string Id) : IRequest;

public record GetPhotosByAlbumQuery(string UserId, string AlbumId) : IRequest<List<EvolutionPhotoDto>>;

public class AddPhotoValidator : AbstractValidator<AddPhotoCommand>
{
    public AddPhotoValidator() => RuleFor(x => x.ImageUrl).NotEmpty();
}

public class AddPhotoHandler(
    IEvolutionAlbumRepository albumRepo,
    IEvolutionPhotoRepository photoRepo,
    ITimelineEventRepository timelineRepo)
    : IRequestHandler<AddPhotoCommand, EvolutionPhotoDto>
{
    public async Task<EvolutionPhotoDto> Handle(AddPhotoCommand cmd, CancellationToken ct)
    {
        var album = await albumRepo.GetByIdAsync(cmd.AlbumId, cmd.UserId, ct)
            ?? throw new NotFoundException("Álbum não encontrado.");

        var photo = new EvolutionPhoto
        {
            UserId = cmd.UserId,
            AlbumId = cmd.AlbumId,
            ImageUrl = cmd.ImageUrl,
            Caption = cmd.Caption,
            Date = cmd.Date.Date,
            Tags = cmd.Tags ?? [],
            LinkedGoalId = cmd.LinkedGoalId,
            LinkedHabitId = cmd.LinkedHabitId,
        };

        if (album.CoverImage is null)
        {
            album.CoverImage = cmd.ImageUrl;
            await albumRepo.UpdateAsync(album, ct);
        }

        await photoRepo.AddAsync(photo, ct);

        await timelineRepo.AddFromModuleAsync(new TimelineEvent
        {
            UserId = cmd.UserId,
            Type = TimelineEventType.EvolutionPhotoAdded,
            Area = album.Area,
            Title = $"Foto adicionada: {album.Title}",
            Description = cmd.Caption,
            OccurredAt = photo.CreatedAt,
            SourceModule = "Evolution",
            SourceId = photo.Id,
            Visibility = TimelineVisibility.Private,
            MediaUrls = [cmd.ImageUrl],
        });

        return photo.ToDto();
    }
}

public class DeletePhotoHandler(IEvolutionPhotoRepository repo)
    : IRequestHandler<DeletePhotoCommand>
{
    public async Task Handle(DeletePhotoCommand cmd, CancellationToken ct)
    {
        var photo = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Foto não encontrada.");
        await repo.DeleteAsync(photo.Id, cmd.UserId, ct);
    }
}

public class GetPhotosByAlbumHandler(IEvolutionPhotoRepository repo)
    : IRequestHandler<GetPhotosByAlbumQuery, List<EvolutionPhotoDto>>
{
    public async Task<List<EvolutionPhotoDto>> Handle(GetPhotosByAlbumQuery q, CancellationToken ct)
    {
        var photos = await repo.GetByAlbumAsync(q.AlbumId, q.UserId);
        return photos.Select(p => p.ToDto()).ToList();
    }
}
