using System.Text.Json;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Aurora.API.Idempotency;

/// <summary>
/// Makes a write endpoint idempotent when the client sends an <c>Idempotency-Key</c> header: the
/// first successful response for a (user, key) pair is cached and replayed for any repeat, and a
/// concurrent duplicate that arrives while the first is still in flight is rejected with 409.
/// Requests without the header are processed normally.
/// </summary>
public class IdempotencyFilter(ICacheService cache, IUserContext user) : IAsyncActionFilter
{
    public const string HeaderName = "Idempotency-Key";
    private static readonly TimeSpan ResultTtl = TimeSpan.FromHours(24);
    private static readonly TimeSpan LockTtl = TimeSpan.FromSeconds(30);

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var header)
            || string.IsNullOrWhiteSpace(header))
        {
            await next();
            return;
        }

        var scope = string.IsNullOrWhiteSpace(user.UserId) ? "anon" : user.UserId;
        var idKey = $"idem:{scope}:{header.ToString().Trim()}";
        var lockKey = $"{idKey}:lock";
        var ct = context.HttpContext.RequestAborted;

        var replay = await cache.GetAsync<StoredResponse>(idKey, ct);
        if (replay is not null)
        {
            context.Result = ToResult(replay);
            return;
        }

        // Claim processing so a simultaneous duplicate can't run the action twice.
        if (!await cache.AcquireLockAsync(lockKey, LockTtl, ct))
        {
            replay = await cache.GetAsync<StoredResponse>(idKey, ct);
            context.Result = replay is not null
                ? ToResult(replay)
                : new ConflictObjectResult(new ApiResponse<string>(false, "Requisição já está sendo processada."));
            return;
        }

        var executed = await next();

        // Only cache successful, serializable responses so retries after a failure can try again.
        if (executed.Result is ObjectResult { Value: not null } obj && (obj.StatusCode is null or (>= 200 and < 300)))
        {
            var stored = new StoredResponse(obj.StatusCode ?? StatusCodes.Status200OK, JsonSerializer.Serialize(obj.Value));
            await cache.SetAsync(idKey, stored, ResultTtl, ct);
        }
    }

    private static ContentResult ToResult(StoredResponse stored) => new()
    {
        StatusCode = stored.StatusCode,
        Content = stored.Body,
        ContentType = "application/json",
    };

    public sealed record StoredResponse(int StatusCode, string Body);
}
