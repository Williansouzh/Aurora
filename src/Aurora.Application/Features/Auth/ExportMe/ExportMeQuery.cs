using Aurora.Application.Abstractions.Persistence;
using Aurora.Application.Abstractions.Security;
using Aurora.Application.Features.Auth.Common;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Auth.ExportMe;

public record ExportMeQuery(string UserId) : IRequest<ExportMeResponse>;

public record ExportMeResponse(
    string UserId,
    string Name,
    string Email,
    bool IsEmailConfirmed,
    bool IsMfaEnabled,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<AuditEntryDto> AuditEntries);

public record AuditEntryDto(string Action, string EntityType, string? EntityId, DateTime OccurredAt, string? Metadata);

public class ExportMeHandler(
    IUserRepository users,
    IAuditLogRepository auditLogs,
    IEncryptionService encryption) : IRequestHandler<ExportMeQuery, ExportMeResponse>
{
    public async Task<ExportMeResponse> Handle(ExportMeQuery query, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(query.UserId)
            ?? throw new NotFoundException("Usuario nao encontrado");
        var audit = await auditLogs.GetByUserAsync(user.Id, ct);

        return new ExportMeResponse(
            user.Id,
            user.Name,
            UserSecurityMapper.ReadEmail(user, encryption),
            user.IsEmailConfirmed,
            user.IsMfaEnabled,
            user.CreatedAt,
            user.UpdatedAt,
            audit.Select(x => new AuditEntryDto(x.Action, x.EntityType, x.EntityId, x.OccurredAt, x.Metadata)).ToList());
    }
}
