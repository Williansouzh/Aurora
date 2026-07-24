using Aurora.API.Authorization;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.EvolutionPhotos.Albums;
using Aurora.Application.Features.EvolutionPhotos.Common;
using Aurora.Application.Features.EvolutionPhotos.Photos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, RequireModule(ModuleKeys.Evolution), Route("api/evolution")]
public class EvolutionController(ISender sender, IUserContext user, IStorageService storage) : ControllerBase
{
    // Albums
    [HttpGet("albums")]
    public async Task<IActionResult> GetAlbums()
    {
        var albums = await sender.Send(new GetAlbumsQuery(user.UserId));
        var resolved = new List<EvolutionAlbumDto>(albums.Count);
        foreach (var album in albums)
        {
            resolved.Add(album with { CoverImage = await storage.ResolveUrlAsync(album.CoverImage) });
        }
        return Ok(new ApiResponse<List<EvolutionAlbumDto>>(true, resolved));
    }

    [HttpPost("albums")]
    public async Task<IActionResult> CreateAlbum(CreateAlbumCommand req)
    {
        var album = await sender.Send(req with { UserId = user.UserId });
        return Ok(new ApiResponse<EvolutionAlbumDto>(true,
            album with { CoverImage = await storage.ResolveUrlAsync(album.CoverImage) }));
    }

    [HttpPut("albums/{id}")]
    public async Task<IActionResult> UpdateAlbum(string id, UpdateAlbumCommand req)
    {
        var album = await sender.Send(req with { UserId = user.UserId, Id = id });
        return Ok(new ApiResponse<EvolutionAlbumDto>(true,
            album with { CoverImage = await storage.ResolveUrlAsync(album.CoverImage) }));
    }

    [HttpDelete("albums/{id}")]
    public async Task<IActionResult> DeleteAlbum(string id)
    {
        await sender.Send(new DeleteAlbumCommand(user.UserId, id));
        return Ok(new ApiResponse<string>(true, "deleted"));
    }

    // Photos
    [HttpGet("albums/{albumId}/photos")]
    public async Task<IActionResult> GetPhotos(string albumId)
    {
        var photos = await sender.Send(new GetPhotosByAlbumQuery(user.UserId, albumId));
        return Ok(new ApiResponse<List<EvolutionPhotoDto>>(true, await ResolvePhotosAsync(photos)));
    }

    [HttpPost("albums/{albumId}/photos")]
    public async Task<IActionResult> AddPhoto(string albumId, AddPhotoCommand req)
    {
        var photo = await sender.Send(req with { UserId = user.UserId, AlbumId = albumId });
        return Ok(new ApiResponse<EvolutionPhotoDto>(true,
            photo with { ImageUrl = await storage.ResolveUrlAsync(photo.ImageUrl) ?? photo.ImageUrl }));
    }

    [HttpDelete("photos/{id}")]
    public async Task<IActionResult> DeletePhoto(string id)
    {
        await sender.Send(new DeletePhotoCommand(user.UserId, id));
        return Ok(new ApiResponse<string>(true, "deleted"));
    }

    private async Task<List<EvolutionPhotoDto>> ResolvePhotosAsync(List<EvolutionPhotoDto> photos)
    {
        var resolved = new List<EvolutionPhotoDto>(photos.Count);
        foreach (var photo in photos)
        {
            resolved.Add(photo with { ImageUrl = await storage.ResolveUrlAsync(photo.ImageUrl) ?? photo.ImageUrl });
        }
        return resolved;
    }
}
