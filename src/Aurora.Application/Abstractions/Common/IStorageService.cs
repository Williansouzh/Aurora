namespace Aurora.Application.Abstractions.Common;

public interface IStorageService
{
    /// <summary>
    /// Uploads a private object scoped to <paramref name="userId"/> and returns the storage
    /// object key (e.g. "{userId}/{guid}_{name}"). The object is NOT publicly readable; callers
    /// must obtain a short-lived URL via <see cref="ResolveUrlAsync"/> to display it.
    /// </summary>
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, string userId, CancellationToken ct = default);

    /// <summary>
    /// Turns a stored value into a URL the browser can load. Object keys are converted into a
    /// short-lived presigned GET URL; legacy absolute URLs are returned unchanged for compatibility.
    /// </summary>
    Task<string?> ResolveUrlAsync(string? storedValueOrKey, CancellationToken ct = default);

    Task DeleteAsync(string storedValueOrKey, CancellationToken ct = default);
}
