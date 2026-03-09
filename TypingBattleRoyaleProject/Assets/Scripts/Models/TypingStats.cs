public class TypingStats
{
    public int Hits { get; private set; }
    public int Errors {  get; private set; }
    public int TotalKeystrokes {  get; private set; }
    public float TimeElapsed {  get; private set; }

    public double GetWPM()
    {
        if (TimeElapsed <= 0)
            return 0;

        return (Hits / 5.0) / (TimeElapsed / 60.0);
    }

    public double GetAccuracy()
    {
        if (TotalKeystrokes <= 0)
            return 0;

        return (Hits / (double)TotalKeystrokes) * 100.0;
    }

}
