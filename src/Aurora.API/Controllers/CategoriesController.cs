using Aurora.API.Authorization;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Categories.Common;
using Aurora.Application.Features.Categories.Create;
using Aurora.Application.Features.Categories.Delete;
using Aurora.Application.Features.Categories.GetAll;
using Aurora.Application.Features.Categories.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, RequireModule(ModuleKeys.Finances), Route("api/categories")]
public class CategoriesController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Categories() =>
        Ok(new ApiResponse<List<CategoryDto>>(true, await sender.Send(new GetCategoriesQuery(user.UserId))));

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryCommand req) =>
        Ok(new ApiResponse<CategoryDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(string id, UpdateCategoryCommand req) =>
        Ok(new ApiResponse<CategoryDto>(true, await sender.Send(req with { UserId = user.UserId, Id = id })));

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        await sender.Send(new DeleteCategoryCommand(user.UserId, id));
        return Ok(new ApiResponse<string>(true, "deleted"));
    }
}
