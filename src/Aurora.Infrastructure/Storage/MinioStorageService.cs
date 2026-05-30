using Aurora.Application.Abstractions.Common;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Aurora.Infrastructure.Storage;

public class MinioStorageService(IOptions<StorageSettings> options) : IStorageService
{
    private readonly StorageSettings _settings = options.Value;

    private IMinioClient CreateClient() =>
        new MinioClient()
            .WithEndpoint(_settings.Endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey)
            .WithSSL(_settings.UseSSL)
            .Build();

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var client = CreateClient();

        var exists = await client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_settings.BucketName), ct);

        if (!exists)
            await client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_settings.BucketName), ct);

        var objectName = $"{Guid.NewGuid():N}_{fileName}";

        await client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType), ct);

        return $"{_settings.PublicUrl}/{_settings.BucketName}/{objectName}";
    }

    public async Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        var objectName = fileUrl.Split('/').Last();
        var client = CreateClient();
        await client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName), ct);
    }
}
