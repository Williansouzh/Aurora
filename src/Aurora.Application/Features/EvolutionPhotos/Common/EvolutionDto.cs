using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.EvolutionPhotos.Common;

public record EvolutionAlbumDto(
    string Id,
    string Title,
    LifeArea Area,
    string? Description,
    string? CoverImage,
    bool IsPrivate,
    DateTime CreatedAt);

public record EvolutionPhotoDto(
    string Id,
    string AlbumId,
    string ImageUrl,
    string? Caption,
    DateTime Date,
    List<string> Tags,
    string? LinkedGoalId,
    string? LinkedHabitId,
    DateTime CreatedAt);

public static class EvolutionMappingExtensions
{
    public static EvolutionAlbumDto ToDto(this EvolutionAlbum a) =>
        new(a.Id, a.Title, a.Area, a.Description, a.CoverImage, a.IsPrivate, a.CreatedAt);

    public static EvolutionPhotoDto ToDto(this EvolutionPhoto p) =>
        new(p.Id, p.AlbumId, p.ImageUrl, p.Caption, p.Date, p.Tags, p.LinkedGoalId, p.LinkedHabitId, p.CreatedAt);
}
