using Aurora.Domain.Entities;
using Aurora.Domain.Enums;

namespace Aurora.Application.Features.Categories.Common;

public record CategoryDto(
    string Id,
    string Name,
    CategoryType Type,
    string Color,
    string Icon,
    bool IsDefault);

public static class CategoryMapper
{
    public static CategoryDto ToDto(this Category category) =>
        new(category.Id, category.Name, category.Type, category.Color, category.Icon, category.IsDefault);
}
