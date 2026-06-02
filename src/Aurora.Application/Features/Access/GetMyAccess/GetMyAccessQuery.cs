using Aurora.Application.Abstractions.Common;
using Aurora.Application.Features.Access.Common;
using MediatR;

namespace Aurora.Application.Features.Access.GetMyAccess;

public record GetMyAccessQuery(string UserId) : IRequest<AccessSnapshotDto>;

public class GetMyAccessHandler(IAccessControlService access)
    : IRequestHandler<GetMyAccessQuery, AccessSnapshotDto>
{
    public Task<AccessSnapshotDto> Handle(GetMyAccessQuery request, CancellationToken ct) =>
        access.GetSnapshotAsync(request.UserId, ct);
}
