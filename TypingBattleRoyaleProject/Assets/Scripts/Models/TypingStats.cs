public class TypingStats
{
    public int hits;
    public int totalKeystrokes;
    public float timeElapsed;

    public float damaged = 10;

    private static readonly (float minAccuracy, float multiplier)[] DamageBonusTable =
    {
        (90f, 1.1f),
        (80f, 1.0f),
        (50f, 0.8f),
        (30f, 0.5f),
        (0f,  0f)
    };

    public float GetWPM()
    {
        if (timeElapsed <= 0f)
            return 0f;

        return (hits / 5f) / (timeElapsed / 60f);
    }

    public float GetAccuracy()
    {
        if (totalKeystrokes <= 0)
            return 0f;

        return (hits / (float)totalKeystrokes) * 100f;
    }

    public float GetDamageBonusMultiplier()
    {
        float accuracy = GetAccuracy();

        foreach (var rule in DamageBonusTable)
        {
            if (accuracy >= rule.minAccuracy)
                return rule.multiplier;
        }

        return 0f;
    }

}
