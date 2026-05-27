using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Categories.Common;
using Aurora.Domain.Entities;
using Aurora.Domain.Enums;
using MediatR;

namespace Aurora.Application.Features.Categories.Create;

public record CreateCategoryCommand(
    string UserId,
    string Name,
    CategoryType Type,
    string Color,
    string Icon) : IRequest<CategoryDto>;

public class CreateCategoryHandler(ICategoryRepository categories) : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand command, CancellationToken ct)
    {
        var category = new Category
        {
            UserId = command.UserId,
            Name = command.Name,
            Type = command.Type,
            Color = command.Color,
            Icon = command.Icon
        };

        await categories.AddAsync(category);
        return category.ToDto();
    }
}
