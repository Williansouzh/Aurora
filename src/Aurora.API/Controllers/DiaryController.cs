using Aurora.API.Authorization;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Common;
using Aurora.Application.Features.Diary.Common;
using Aurora.Application.Features.Diary.Create;
using Aurora.Application.Features.Diary.Delete;
using Aurora.Application.Features.Diary.GetByDate;
using Aurora.Application.Features.Diary.GetPaged;
using Aurora.Application.Features.Diary.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.API.Controllers;

[ApiController, Authorize, RequireModule(ModuleKeys.Diary), Route("api/diary")]
public class DiaryController(ISender sender, IUserContext user) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? search,
        [FromQuery] int? mood,
        [FromQuery] string? tag,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20) =>
        Ok(new ApiResponse<PagedResultDto<DiaryEntryDto>>(true,
            await sender.Send(new GetDiaryEntriesQuery(user.UserId, search, mood, tag, page, pageSize))));

    [HttpGet("date/{date}")]
    public async Task<IActionResult> GetByDate(DateTime date) =>
        Ok(new ApiResponse<DiaryEntryDto?>(true,
            await sender.Send(new GetDiaryEntryByDateQuery(user.UserId, date))));

    [HttpPost]
    public async Task<IActionResult> Create(CreateDiaryEntryCommand req) =>
        Ok(new ApiResponse<DiaryEntryDto>(true, await sender.Send(req with { UserId = user.UserId })));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateDiaryEntryCommand req) =>
        Ok(new ApiResponse<DiaryEntryDto>(true, await sender.Send(req with { UserId = user.UserId, Id = id })));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await sender.Send(new DeleteDiaryEntryCommand(user.UserId, id));
        return Ok(new ApiResponse<string>(true, "deleted"));
    }
}
