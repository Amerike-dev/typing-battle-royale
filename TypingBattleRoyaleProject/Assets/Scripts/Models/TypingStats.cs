public class TypingStats
{
    public int Hits { get; private set; }
    public int Errors {  get; private set; }
    public int TotalKeystrokes {  get; private set; }
    public float TimeElapsed {  get; private set; }

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

}
