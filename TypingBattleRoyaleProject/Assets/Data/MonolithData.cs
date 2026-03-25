using UnityEngine;

public class MonolithData
{
    [SerializeField] private string _ID;
    [SerializeField] private int _level;
    [SerializeField] private bool _isExhausted;
    private string _runeChallenge;
    public SpellData spellData;

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
