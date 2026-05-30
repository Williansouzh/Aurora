using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aurora.Domain.Entities;

[BsonIgnoreExtraElements]
public class Milestone
{
    [BsonId, BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public void Complete()
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        IsCompleted = false;
        CompletedAt = null;
    }
}
