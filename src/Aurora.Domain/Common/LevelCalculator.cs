namespace Aurora.Domain.Common;

public static class LevelCalculator
{
    // XP required to reach level N: 50 * N * (N-1)
    // Level 1=0, Level 5=1000, Level 10=4500, Level 20=19000, Level 30=43500, Level 50=122500

    public static int Compute(int totalXp)
    {
        var level = 1;
        while (XpRequired(level + 1) <= totalXp)
            level++;
        return Math.Min(level, 50);
    }

    public static int XpRequired(int level) => 50 * level * (level - 1);

    public static int XpToNext(int totalXp)
    {
        var level = Compute(totalXp);
        if (level >= 50) return 0;
        return XpRequired(level + 1) - totalXp;
    }

    public static string LevelName(int level) => level switch
    {
        >= 50 => "Modo Aurora",
        >= 30 => "Em evolução",
        >= 20 => "Focado",
        >= 10 => "Consistente",
        >= 5  => "Organizado",
        _     => "Início",
    };
}
