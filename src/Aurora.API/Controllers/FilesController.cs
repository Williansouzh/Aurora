using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, Route("api/files")]
public class FilesController(IStorageService storage) : ControllerBase
{
    private static readonly HashSet<string> AllowedTypes =
        ["image/jpeg", "image/png", "image/webp", "image/gif"];

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ApiResponse<string>(false, "Arquivo não enviado."));

        if (!AllowedTypes.Contains(file.ContentType))
            return BadRequest(new ApiResponse<string>(false, "Tipo de arquivo não permitido. Use JPEG, PNG, WebP ou GIF."));

        await using var stream = file.OpenReadStream();
        var url = await storage.UploadAsync(stream, file.FileName, file.ContentType, ct);

        return Ok(new ApiResponse<UploadResult>(true, new UploadResult(url)));
    }
}

public record UploadResult(string Url);
