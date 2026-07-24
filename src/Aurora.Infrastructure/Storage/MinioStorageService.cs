using System.Text.RegularExpressions;
using Aurora.Application.Abstractions.Common;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Aurora.Infrastructure.Storage;

public partial class MinioStorageService(IOptions<StorageSettings> options) : IStorageService
{
    private static readonly TimeSpan PresignedUrlLifetime = TimeSpan.FromMinutes(15);
    private readonly StorageSettings _settings = options.Value;

    private IMinioClient CreateClient() =>
        new MinioClient()
            .WithEndpoint(_settings.Endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithSSL(_settings.UseSSL)
            .Build();

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string userId, CancellationToken ct = default)
    {
        var client = CreateClient();

        var exists = await client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_settings.BucketName), ct);

        if (!exists)
            await client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_settings.BucketName), ct);

        // Keys are prefixed with the owner id so ownership is auditable, and the file name is
        // sanitized so it can never traverse paths or inject characters into the object key.
        var safeName = SanitizeFileName(fileName);
        var objectName = $"{userId}/{Guid.NewGuid():N}_{safeName}";

        await client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType), ct);

        return objectName;
    }

    public async Task<string?> ResolveUrlAsync(string? storedValueOrKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(storedValueOrKey))
        {
            return storedValueOrKey;
        }

        // Legacy records stored a full absolute URL; return those unchanged.
        if (storedValueOrKey.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || storedValueOrKey.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return storedValueOrKey;
        }

        var client = CreateClient();
        return await client.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(storedValueOrKey)
            .WithExpiry((int)PresignedUrlLifetime.TotalSeconds));
    }

    public async Task DeleteAsync(string storedValueOrKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(storedValueOrKey))
        {
            return;
        }

        var objectName = ExtractObjectKey(storedValueOrKey);
        var client = CreateClient();
        await client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName), ct);
    }

    private string ExtractObjectKey(string storedValueOrKey)
    {
        // Support both new object keys ("{userId}/{guid}_{name}") and legacy absolute URLs
        // ("{publicUrl}/{bucket}/{userId}/{guid}_{name}").
        if (!storedValueOrKey.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return storedValueOrKey;
        }

        var marker = $"/{_settings.BucketName}/";
        var idx = storedValueOrKey.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return idx >= 0
            ? storedValueOrKey[(idx + marker.Length)..]
            : storedValueOrKey.Split('/').Last();
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName ?? string.Empty);
        name = UnsafeChars().Replace(name, "_");
        return string.IsNullOrWhiteSpace(name) ? "file" : name;
    }

    [GeneratedRegex("[^A-Za-z0-9._-]")]
    private static partial Regex UnsafeChars();
}
