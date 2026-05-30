using Aurora.Domain.Entities;

namespace Aurora.Application.Features.Diary.Common;

public record DiaryEntryDto(
    string Id,
    DateTime Date,
    string Content,
    int Mood,
    List<string> Tags,
    List<string> Photos,
    bool IsPrivate,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public static class DiaryMappingExtensions
{
    public static DiaryEntryDto ToDto(this DiaryEntry e) => new(
        e.Id, e.Date, e.Content, e.Mood, e.Tags, e.Photos,
        e.IsPrivate, e.CreatedAt, e.UpdatedAt);
}
