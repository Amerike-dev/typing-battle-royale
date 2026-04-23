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
    public Action OnDamageTaken;
    public Action OnEnemyKilled;

    public string ID  => _id;
    public float currentHP => _currentHP;
    public float maxHP => _maxHP;
    public int maxLives => _maxLives;
    public bool isAlive => _isAlive;
    public int killCount => _killCount;
    public float wPM => _wpm;

    public PlayerStats(string id)
    {
        _id = id;
    }

    public void EnemyKilled()
    {
        OnEnemyKilled?.Invoke();
    }

    public void TakeDamage(float damage)
    {
        _currentHP -= damage;
        OnDamageTaken?.Invoke();
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

}
