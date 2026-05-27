using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Features.Categories.Common;
using MediatR;

namespace Aurora.Application.Features.Categories.GetAll;

public record GetCategoriesQuery(string UserId) : IRequest<List<CategoryDto>>;

public class GetCategoriesHandler(ICategoryRepository categories) : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery query, CancellationToken ct) =>
        (await categories.GetByUserAsync(query.UserId)).Select(x => x.ToDto()).ToList();
}
