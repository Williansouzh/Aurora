using Aurora.Domain.Common;
using Aurora.Domain.Exceptions;

namespace Aurora.Domain.Entities;

public class DiaryEntry : EntityBase, IUserOwned
{
    public string UserId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Mood { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<string> Photos { get; set; } = [];
    public bool IsPrivate { get; set; } = true;

    public void Update(string content, int mood, List<string> tags)
    {
        if (mood is < 1 or > 5)
            throw new ValidationException("Humor deve ser entre 1 e 5.");
        Content = content;
        Mood = mood;
        Tags = tags;
        UpdatedAt = DateTime.UtcNow;
    }
}
