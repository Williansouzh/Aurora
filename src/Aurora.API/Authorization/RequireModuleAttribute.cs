using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Aurora.API.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireModuleAttribute(string moduleKey, string action = "read") : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            context.Result = new UnauthorizedObjectResult(new ApiResponse<string>(false, "unauthorized"));
            return;
        }

        var access = context.HttpContext.RequestServices.GetRequiredService<IAccessControlService>();
        var decision = await access.CanAccessModuleAsync(userId, moduleKey, action, context.HttpContext.RequestAborted);
        if (decision.IsAllowed) return;

        context.Result = new ObjectResult(new ApiResponse<object>(
            false,
            new
            {
                module = moduleKey,
                reason = decision.Reason.ToString()
            },
            "Este modulo ainda nao esta disponivel para o seu acesso."))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}
