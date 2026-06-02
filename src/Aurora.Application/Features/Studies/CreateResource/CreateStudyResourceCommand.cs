using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Studies.CreateResource;

public record CreateStudyResourceCommand(
    string UserId,
    string SkillId,
    string Title,
    StudyResourceType Type,
    string? Url,
    string? Author,
    int Reliability,
    int SortOrder,
    string? Notes) : IRequest<StudyResourceDto>;

public class CreateStudyResourceValidator : AbstractValidator<CreateStudyResourceCommand>
{
    public CreateStudyResourceValidator()
    {
        RuleFor(x => x.SkillId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(180);
        RuleFor(x => x.Reliability).InclusiveBetween(1, 5);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class CreateStudyResourceHandler(IStudyResourceRepository resources, IStudySkillRepository skills)
    : IRequestHandler<CreateStudyResourceCommand, StudyResourceDto>
{
    public async Task<StudyResourceDto> Handle(CreateStudyResourceCommand cmd, CancellationToken ct)
    {
        var skill = await skills.GetByIdAsync(cmd.SkillId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Habilidade de estudo nao encontrada.");

        var resource = new StudyResource
        {
            UserId = cmd.UserId,
            SkillId = cmd.SkillId,
            Title = cmd.Title.Trim(),
            Type = cmd.Type,
            Url = string.IsNullOrWhiteSpace(cmd.Url) ? null : cmd.Url.Trim(),
            Author = string.IsNullOrWhiteSpace(cmd.Author) ? null : cmd.Author.Trim(),
            Reliability = cmd.Reliability,
            SortOrder = cmd.SortOrder,
            Notes = string.IsNullOrWhiteSpace(cmd.Notes) ? null : cmd.Notes.Trim(),
            Status = StudyResourceStatus.Planned
        };

        await resources.AddAsync(resource, ct);
        return resource.ToDto(skill.Title);
    }
}

