using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Studies.Common;
using Aurora.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Features.Studies.UpdateResource;

public record UpdateStudyResourceCommand(
    string UserId,
    string ResourceId,
    string Title,
    StudyResourceType Type,
    StudyResourceStatus Status,
    string? Url,
    string? Author,
    int Reliability,
    int SortOrder,
    string? Notes) : IRequest<StudyResourceDto>;

public class UpdateStudyResourceValidator : AbstractValidator<UpdateStudyResourceCommand>
{
    public UpdateStudyResourceValidator()
    {
        RuleFor(x => x.ResourceId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(180);
        RuleFor(x => x.Reliability).InclusiveBetween(1, 5);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateStudyResourceHandler(IStudyResourceRepository resources, IStudySkillRepository skills)
    : IRequestHandler<UpdateStudyResourceCommand, StudyResourceDto>
{
    public async Task<StudyResourceDto> Handle(UpdateStudyResourceCommand cmd, CancellationToken ct)
    {
        var resource = await resources.GetByIdAsync(cmd.ResourceId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Recurso de estudo nao encontrado.");
        var skill = await skills.GetByIdAsync(resource.SkillId, cmd.UserId, ct)
            ?? throw new InvalidOperationException("Habilidade de estudo nao encontrada.");

        resource.Title = cmd.Title.Trim();
        resource.Type = cmd.Type;
        resource.Status = cmd.Status;
        resource.Url = string.IsNullOrWhiteSpace(cmd.Url) ? null : cmd.Url.Trim();
        resource.Author = string.IsNullOrWhiteSpace(cmd.Author) ? null : cmd.Author.Trim();
        resource.Reliability = cmd.Reliability;
        resource.SortOrder = cmd.SortOrder;
        resource.Notes = string.IsNullOrWhiteSpace(cmd.Notes) ? null : cmd.Notes.Trim();
        await resources.UpdateAsync(resource, ct);
        return resource.ToDto(skill.Title);
    }
}

