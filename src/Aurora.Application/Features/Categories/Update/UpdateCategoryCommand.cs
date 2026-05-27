using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Categories.Common;
using Aurora.Domain.Enums;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Categories.Update;

public record UpdateCategoryCommand(
    string UserId,
    string Id,
    string Name,
    CategoryType Type,
    string Color,
    string Icon) : IRequest<CategoryDto>;

public class UpdateCategoryHandler(ICategoryRepository categories) : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand command, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(command.Id, command.UserId)
            ?? throw new NotFoundException("Categoria nao encontrada");

        category.Name = command.Name;
        category.Type = command.Type;
        category.Color = command.Color;
        category.Icon = command.Icon;

        await categories.UpdateAsync(category);
        return category.ToDto();
    }
}
