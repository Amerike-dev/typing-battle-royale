using UnityEngine;

public class testTypingStats : MonoBehaviour
{
    public int Hits { get; set; }
    public int Errors {  get; private set; }
    public int TotalKeystrokes {  get; set; }
    public float TimeElapsed {  get; set; }

    void Start()
    {
        testTypingStats stats = new testTypingStats();

        stats.Hits = 50;
        stats.TotalKeystrokes = 300;
        stats.TimeElapsed = 100f;

        Debug.Log("WPM: " + stats.GetWPM());
        Debug.Log("Accuracy: " + stats.GetAccuracy());
    }

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
