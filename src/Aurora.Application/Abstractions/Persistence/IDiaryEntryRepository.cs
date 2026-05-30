using Aurora.Domain.Entities;

namespace Aurora.Application.Abstractions.Persistence;

public interface IDiaryEntryRepository : IRepository<DiaryEntry>
{
    Task<DiaryEntry?> GetByDateAsync(string userId, DateTime date);
    Task<(List<DiaryEntry> Items, int TotalCount)> GetPagedAsync(
        string userId, string? search, int? mood, string? tag, int page, int pageSize);
    Task<List<DiaryEntry>> GetRecentAsync(string userId, int limit = 7);
}
