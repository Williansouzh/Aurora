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
public class EvolutionController(ISender sender, IUserContext user) : ControllerBase
{
    // Albums
    [HttpGet("albums")]
    public async Task<IActionResult> GetAlbums() =>
        Ok(new ApiResponse<List<EvolutionAlbumDto>>(true,
            await sender.Send(new GetAlbumsQuery(user.UserId))));

    [HttpPost("albums")]
    public async Task<IActionResult> CreateAlbum(CreateAlbumCommand req) =>
        Ok(new ApiResponse<EvolutionAlbumDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpPut("albums/{id}")]
    public async Task<IActionResult> UpdateAlbum(string id, UpdateAlbumCommand req) =>
        Ok(new ApiResponse<EvolutionAlbumDto>(true, await sender.Send(req with { UserId = user.UserId, Id = id })));

    [HttpDelete("albums/{id}")]
    public async Task<IActionResult> DeleteAlbum(string id)
    {
        await sender.Send(new DeleteAlbumCommand(user.UserId, id));
        return Ok(new ApiResponse<string>(true, "deleted"));
    }

    // Photos
    [HttpGet("albums/{albumId}/photos")]
    public async Task<IActionResult> GetPhotos(string albumId) =>
        Ok(new ApiResponse<List<EvolutionPhotoDto>>(true,
            await sender.Send(new GetPhotosByAlbumQuery(user.UserId, albumId))));

    [HttpPost("albums/{albumId}/photos")]
    public async Task<IActionResult> AddPhoto(string albumId, AddPhotoCommand req) =>
        Ok(new ApiResponse<EvolutionPhotoDto>(true,
            await sender.Send(req with { UserId = user.UserId, AlbumId = albumId })));

    [HttpDelete("photos/{id}")]
    public async Task<IActionResult> DeletePhoto(string id)
    {
        await sender.Send(new DeletePhotoCommand(user.UserId, id));
        return Ok(new ApiResponse<string>(true, "deleted"));
    }
}
