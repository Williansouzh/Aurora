namespace Aurora.API.Files;

/// <summary>
/// Validates that an uploaded file's leading bytes match a supported raster image signature, so a
/// renamed non-image (declaring an image content type) can't be stored and later served as one.
/// </summary>
public static class ImageContentValidator
{
    public const int HeaderSize = 12;

    public static bool IsAllowedImage(ReadOnlySpan<byte> header)
    {
        if (header.Length < HeaderSize)
        {
            return false;
        }

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
