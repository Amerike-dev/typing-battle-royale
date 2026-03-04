using UnityEngine;

public class MonolithData
{
    private string _ID;
    private int _level;
    private string _runeChallenge;

    public MonolithData(string ID, int level, string runeChallenge)
    {
        this._ID = ID;
        this.Level = level;
        this._runeChallenge = runeChallenge;
    }

    public int Level
    {
        get { return _level; }

        set
        {
            if (value < 4 && value > 0)
            {
                _level = value;
            }
            else
            {
                Debug.Log("Value not valid. Setting Default (1)");
                _level = 1;
            }
        }
    }
}
