public class TypingStats
{
    public int hits;
    public int totalKeystrokes;
    public float timeElapsed;

    public float damaged = 10;

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

        switch (accuracy)
        {
            case > 90f:
                return 1.1f;
            case >= 80f:
                return 1f;
            case >= 50f:
                return 0.8f;
            case >= 30f:
                return 0.5f;
            default:
                return 0f;
        }
    }

}
