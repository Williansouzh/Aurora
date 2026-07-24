using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, Route("api/files")]
public class FilesController(IStorageService storage, IUserContext user) : ControllerBase
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

        // Don't trust the declared content type: verify the real file signature (magic bytes) so a
        // renamed executable/HTML cannot be stored and later served as an "image".
        if (!await IsAllowedImageAsync(stream, ct))
            return BadRequest(new ApiResponse<string>(false, "O conteúdo do arquivo não corresponde a uma imagem válida."));

        stream.Position = 0;
        var key = await storage.UploadAsync(stream, file.FileName, file.ContentType, user.UserId, ct);
        var previewUrl = await storage.ResolveUrlAsync(key, ct);

        return Ok(new ApiResponse<UploadResult>(true, new UploadResult(key, previewUrl)));
    }

    private static async Task<bool> IsAllowedImageAsync(Stream stream, CancellationToken ct)
    {
        var header = new byte[12];
        var read = await stream.ReadAsync(header.AsMemory(0, header.Length), ct);
        if (read < 12) return false;

        // JPEG
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;
        // PNG
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return true;
        // GIF ("GIF8")
        if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38) return true;
        // WebP ("RIFF"...."WEBP")
        if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
            && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50) return true;

        return false;
    }
}

public record UploadResult(string Key, string? Url);
