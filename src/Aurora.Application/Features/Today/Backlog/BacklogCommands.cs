using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Common;
using Aurora.Application.Features.Today.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Today.Backlog;

public record GetBacklogQuery(string UserId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResultDto<DailyTaskDto>>;

public record AddToBacklogCommand(
    string UserId,
    string Title,
    string? Notes,
    DailyTaskPriority Priority) : IRequest<DailyTaskDto>;

public record MoveToTodayCommand(string UserId, string Id) : IRequest<DailyTaskDto>;

public class AddToBacklogValidator : AbstractValidator<AddToBacklogCommand>
{
    public AddToBacklogValidator() => RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
}

public class GetBacklogHandler(IDailyTaskRepository repo)
    : IRequestHandler<GetBacklogQuery, PagedResultDto<DailyTaskDto>>
{
    public async Task<PagedResultDto<DailyTaskDto>> Handle(GetBacklogQuery q, CancellationToken ct)
    {
        var page = q.Page < 1 ? 1 : q.Page;
        var pageSize = q.PageSize < 1 ? 20 : q.PageSize > 100 ? 100 : q.PageSize;

        var (items, total) = await repo.GetBacklogPagedAsync(q.UserId, page, pageSize);
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return new PagedResultDto<DailyTaskDto>(
            items.Select(t => t.ToDto()).ToList(), total, page, pageSize, totalPages);
    }
}

public class AddToBacklogHandler(IDailyTaskRepository repo)
    : IRequestHandler<AddToBacklogCommand, DailyTaskDto>
{
    public async Task<DailyTaskDto> Handle(AddToBacklogCommand cmd, CancellationToken ct)
    {
        var task = new DailyTask
        {
            UserId = cmd.UserId,
            Title = cmd.Title,
            Notes = cmd.Notes,
            Priority = cmd.Priority,
            Status = DailyTaskStatus.Pending,
            IsBacklog = true,
            Date = DateTime.UtcNow.Date,
        };
        await repo.AddAsync(task, ct);
        return task.ToDto();
    }
}

public class MoveToTodayHandler(IDailyTaskRepository repo)
    : IRequestHandler<MoveToTodayCommand, DailyTaskDto>
{
    public async Task<DailyTaskDto> Handle(MoveToTodayCommand cmd, CancellationToken ct)
    {
        var task = await repo.GetByIdAsync(cmd.Id, cmd.UserId, ct)
            ?? throw new NotFoundException("Tarefa não encontrada.");
        task.IsBacklog = false;
        task.Date = DateTime.UtcNow.Date;
        task.UpdatedAt = DateTime.UtcNow;
        await repo.UpdateAsync(task, ct);
        return task.ToDto();
    }
}
