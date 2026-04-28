using System;
using UnityEngine;

[System.Serializable]
public class PlayerStats
{

    [SerializeField]private string _id;
    
    [SerializeField] private float _maxHP;
    private float _currentHP;
    [SerializeField]private int _maxLives;

    private int _currentLives;
    private int _killCount;
    private bool _isAlive;
    private float _wpm;
    
    public Action OnLifeLost;
    public Action OnAllLifeLost;

    public string ID  => _id;
    public float currentHP => _currentHP;
    public float maxHP => _maxHP;
    public int maxLives => _maxLives;
    public int currentLives => _currentLives;
    public bool isAlive => _isAlive;
    public int killCount => _killCount;
    public float wPM => _wpm;

    public PlayerStats(string id)
    {
        _id = id;
    }

    public void TakeDamage(float damage)
    {
        _currentHP -= damage;
        if (_currentHP <= 0)
            LoseLife();
            
    }
    public void LoseLife()
    {
        _currentHP = _maxHP;
        _currentLives--;
        _isAlive = _currentLives > 0;
        OnLifeLost?.Invoke();
        if (!isAlive)
            OnAllLifeLost?.Invoke();
        else
            return;
    }

    public void Initialize()
{
    _currentHP = _maxHP;
    _currentLives = _maxLives;
    _isAlive = true;
}

}
