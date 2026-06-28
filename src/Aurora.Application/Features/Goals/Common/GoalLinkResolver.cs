using Aurora.Application.Abstractions.Persistence;

namespace Aurora.Application.Features.Goals.Common;

/// <summary>
/// Loads id→name maps for the rituals (habits) and study skills a goal can be linked to,
/// so <see cref="GoalMappingExtensions"/> can resolve linked refs to display names.
/// </summary>
public static class GoalLinkResolver
{
    public static async Task<(IReadOnlyDictionary<string, string> Habits, IReadOnlyDictionary<string, string> Skills)>
        LoadAsync(string userId, IHabitRepository habits, IStudySkillRepository skills, CancellationToken ct)
    {
        var habitList = await habits.GetByUserAsync(userId, ct);
        var skillList = await skills.GetByUserAsync(userId, ct);
        var habitNames = habitList.ToDictionary(h => h.Id, h => h.Name);
        var skillNames = skillList.ToDictionary(s => s.Id, s => s.Title);
        return (habitNames, skillNames);
    }
}
