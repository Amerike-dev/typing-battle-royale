public class TypingStats
{
    public int hits;
    public int totalKeystrokes;
    public float timeElapsed;

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

}
