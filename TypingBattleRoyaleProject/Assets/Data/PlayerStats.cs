using UnityEngine;

public class PlayerStats
{

    private string _id;
    private int _health;
    private float _currentHP;
    private float _maxHP; 

    private int _killCount;
    private float _wpm;

    public PlayerStats(string id, int health)
    {
        this._id = id;
        this._health = health;
        _maxHP = health;
        _currentHP = health;
    }

    public string ID {  get { return _id; } }

    public float CurrentHP => _currentHP;
    public float MaxHP => _maxHP;

    public int KillCount => _killCount;
    public float WPM => _wpm;
}
