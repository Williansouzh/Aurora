using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Idempotency;

/// <summary>Applies <see cref="IdempotencyFilter"/> to an action (honours the Idempotency-Key header).</summary>
public sealed class IdempotentAttribute() : TypeFilterAttribute(typeof(IdempotencyFilter));
