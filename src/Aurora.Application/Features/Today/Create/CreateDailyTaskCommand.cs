using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Today.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Today.Create;

public record CreateDailyTaskCommand(
    string UserId,
    string Title,
    string? Notes,
    DailyTaskPriority Priority,
    DateTime Date) : IRequest<DailyTaskDto>;

public class CreateDailyTaskValidator : AbstractValidator<CreateDailyTaskCommand>
{
    public CreateDailyTaskValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

public class CreateDailyTaskHandler(IDailyTaskRepository repo)
    : IRequestHandler<CreateDailyTaskCommand, DailyTaskDto>
{
    public async Task<DailyTaskDto> Handle(CreateDailyTaskCommand cmd, CancellationToken ct)
    {
        var task = new DailyTask
        {
            UserId = cmd.UserId,
            Title = cmd.Title,
            Notes = cmd.Notes,
            Priority = cmd.Priority,
            Date = cmd.Date.Date,
            Status = DailyTaskStatus.Pending,
        };

        await repo.AddAsync(task, ct);
        return task.ToDto();
    }
}
