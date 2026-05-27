using System.Security.Claims;
using Aurora.Application.Abstractions.Common;

namespace Aurora.API.Extensions;

public class UserContext(IHttpContextAccessor accessor) : IUserContext
{
    public string UserId => accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
