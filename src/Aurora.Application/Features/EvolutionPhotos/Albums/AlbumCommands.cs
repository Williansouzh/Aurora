using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.EvolutionPhotos.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.EvolutionPhotos.Albums;

public record CreateAlbumCommand(
    string UserId,
    string Title,
    LifeArea Area,
    string? Description,
    bool IsPrivate) : IRequest<EvolutionAlbumDto>;

public record UpdateAlbumCommand(
    string UserId,
    string Id,
    string Title,
    LifeArea Area,
    string? Description,
    bool IsPrivate) : IRequest<EvolutionAlbumDto>;

public record DeleteAlbumCommand(string UserId, string Id) : IRequest;

public record GetAlbumsQuery(string UserId) : IRequest<List<EvolutionAlbumDto>>;

public class CreateAlbumValidator : AbstractValidator<CreateAlbumCommand>
{
    public CreateAlbumValidator() => RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
}

public class CreateAlbumHandler(IEvolutionAlbumRepository repo)
    : IRequestHandler<CreateAlbumCommand, EvolutionAlbumDto>
{
    public async Task<EvolutionAlbumDto> Handle(CreateAlbumCommand cmd, CancellationToken ct)
    {
        var album = new EvolutionAlbum
        {
            UserId = cmd.UserId,
            Title = cmd.Title,
            Area = cmd.Area,
            Description = cmd.Description,
            IsPrivate = cmd.IsPrivate,
        };
        await repo.AddAsync(album, ct);
        return album.ToDto();
    }
}

public class UpdateAlbumHandler(IEvolutionAlbumRepository repo)
    : IRequestHandler<UpdateAlbumCommand, EvolutionAlbumDto>
{
    public async Task<EvolutionAlbumDto> Handle(UpdateAlbumCommand cmd, CancellationToken ct)
    {
        var album = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Álbum não encontrado.");
        album.Title = cmd.Title;
        album.Area = cmd.Area;
        album.Description = cmd.Description;
        album.IsPrivate = cmd.IsPrivate;
        album.UpdatedAt = DateTime.UtcNow;
        await repo.UpdateAsync(album, ct);
        return album.ToDto();
    }
}

public class DeleteAlbumHandler(IEvolutionAlbumRepository albumRepo, IEvolutionPhotoRepository photoRepo)
    : IRequestHandler<DeleteAlbumCommand>
{
    public async Task Handle(DeleteAlbumCommand cmd, CancellationToken ct)
    {
        var album = await albumRepo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Álbum não encontrado.");
        await photoRepo.DeleteByAlbumAsync(album.Id, cmd.UserId);
        await albumRepo.DeleteAsync(album.Id, cmd.UserId, ct);
    }
}

public class GetAlbumsHandler(IEvolutionAlbumRepository repo)
    : IRequestHandler<GetAlbumsQuery, List<EvolutionAlbumDto>>
{
    public async Task<List<EvolutionAlbumDto>> Handle(GetAlbumsQuery q, CancellationToken ct)
    {
        var albums = await repo.GetAllAsync(q.UserId);
        return albums.Select(a => a.ToDto()).ToList();
    }
}
