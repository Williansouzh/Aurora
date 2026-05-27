namespace Aurora.Domain.Common;

/// <summary>
/// Marker interface for aggregates owned by a single user (multi-tenant by UserId).
/// Used by <c>MongoRepositoryBase</c> to enforce automatic tenant filtering.
/// </summary>
public interface IUserOwned
{
    string UserId { get; }
}
