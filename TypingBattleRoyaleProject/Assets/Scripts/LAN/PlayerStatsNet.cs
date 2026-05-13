using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Collections;

public class PlayerStatsNet : NetworkBehaviour
{
    [Header("Config")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private int maxLives = 3;

    public float MaxHP => maxHP;
    public int MaxLives => maxLives;

    public NetworkVariable<float> currentHP = new (
        100f, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> currentLifes = new(
        3,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> killCount = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> isAlive = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    public NetworkVariable<float> wPM = new(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    public NetworkVariable<FixedString32Bytes> networkPlayerID = new(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    public string ID => networkPlayerID.Value.ToString();

    public Action OnLifeLost;
    public Action OnAllLifeLost;
    public Action OnDamageTaken;
    public Action OnEnemyKilled;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHP.Value = maxHP;
            currentLifes.Value = maxLives;
            killCount.Value = 0;
            isAlive.Value = true;
        }

        currentHP.OnValueChanged += HandleHPChanged;
        currentLifes.OnValueChanged += HandleLivesChanged;
        killCount.OnValueChanged += HandleKillCountChanged;
    }

    public override void OnNetworkDespawn()
    {
        currentHP.OnValueChanged -= HandleHPChanged;
        currentLifes.OnValueChanged -= HandleLivesChanged;
        killCount.OnValueChanged -= HandleKillCountChanged;
    }

    private void HandleHPChanged(float oldValue, float newValue)
    {
        if (newValue < oldValue)
        {
            OnDamageTaken?.Invoke();
        }
    }

    private void HandleLivesChanged(int oldValue, int newValue)
    {
        if (newValue < oldValue)
        {
            OnLifeLost?.Invoke();
        }

        if (newValue <= 0)
        {
            if (IsServer) isAlive.Value = false; 
            
            OnAllLifeLost?.Invoke();
        }
    }

    private void HandleKillCountChanged(int oldValue, int newValue)
    {
        if (newValue > oldValue)
        {
            OnEnemyKilled?.Invoke();
        }
    }
}
