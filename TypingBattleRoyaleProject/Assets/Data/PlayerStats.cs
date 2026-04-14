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
    private float _wpm;
    public bool _isAlive;


    public Action OnLifeLost;
    public Action OnAllLifeLost;

    public string iD  => _id;
    public float currentHP => _currentHP;
    public float maxHP => _maxHP;
    public int maxLives => _maxLives;
   
    public int killCount => _killCount;
    public float wPM => _wpm;
    
    public void TakeDamage(float damage)
    {
        _currentHP -= damage;
        if (_currentHP <= 0)
            LoseLife();
            
    }
    public void LoseLife() 
    {
        
    }
}
