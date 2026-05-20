using Aurora.Application.Interfaces;
using System.Security.Claims;
namespace Aurora.API.Extensions;
public class UserContext(IHttpContextAccessor accessor): IUserContext { public string UserId => accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty; }
