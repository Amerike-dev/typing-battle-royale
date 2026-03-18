[System.Serializable]
public class MatchSessionData
{
    public string MatchID;
    public float Timer;
    public bool IsActive;

    public MatchSessionData(string matchID)
    {
        MatchID = matchID;
        Timer = 0f;
        IsActive = false;
    }
}