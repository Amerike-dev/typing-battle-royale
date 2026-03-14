public class TypingStats
{
    public int Hits { get; private set; }
    public int Errors {  get; private set; }
    public int TotalKeystrokes {  get; private set; }
    public float TimeElapsed {  get; private set; }

    public float damaged = 10;

    public float GetWPM()
    {
        if (TimeElapsed <= 0f)
            return 0f;

        return (Hits / 5f) / (TimeElapsed / 60f);
    }

    public float GetAccuracy()
    {
        if (TotalKeystrokes <= 0)
            return 0f;

        return (Hits / (float)TotalKeystrokes) * 100f;
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
