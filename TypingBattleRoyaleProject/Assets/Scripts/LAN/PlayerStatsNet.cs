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
        
        if (IsServer) TargetSystem.OnLookDead += HandleGlobalDamageReceived;
    }

    public override void OnNetworkDespawn()
    {
        currentHP.OnValueChanged -= HandleHPChanged;
        currentLifes.OnValueChanged -= HandleLivesChanged;
        killCount.OnValueChanged -= HandleKillCountChanged;
        
        if (IsServer) TargetSystem.OnLookDead -= HandleGlobalDamageReceived;
    }
    
    private void HandleGlobalDamageReceived(string targetID, float damage, ulong attackerId)
    {
        if (!IsServer) return;

        Debug.Log($"[PlayerStatsNet] Evento recibido. this.ID={ID}, targetID={targetID}, damage={damage}, attackerId={attackerId}");

        
        if (this.ID == targetID) 
        {
            Debug.Log($"[PlayerStatsNet] ID coincide. Aplicando daño a {ID}");
            TakeDamage(damage, attackerId);
        }
    }

    private void HandleHPChanged(float oldValue, float newValue)
    {
        if (newValue < oldValue) OnDamageTaken?.Invoke();
    }

    private void HandleLivesChanged(int oldValue, int newValue)
    {
        if (newValue < oldValue) OnLifeLost?.Invoke();

        if (newValue <= 0)
        {
            if (IsServer) isAlive.Value = false; 
            
            OnAllLifeLost?.Invoke();
        }
    }

    private void HandleKillCountChanged(int oldValue, int newValue)
    {
        if (newValue > oldValue) OnEnemyKilled?.Invoke();
    }

    public void TakeDamage(float damage, ulong attackerId = 0)
    {
        if (!IsServer || !isAlive.Value) return;

        Debug.Log($"[STATS] TakeDamage({damage}) on {ID}");
        currentHP.Value -= damage;

        if (currentHP.Value <= 0) HandleDeath(attackerId);
    }

    private void HandleDeath(ulong killerId)
    {
        if (currentLifes.Value > 1)
        {
            currentLifes.Value--;
            currentHP.Value = maxHP;
            RespawnOwnerClientRpc();
        }
        else
        {
            currentLifes.Value = 0;
            currentHP.Value = 0;
            isAlive.Value = false;

            if (IsServer) AwardKillTo(killerId);
        }
    }

    private void AwardKillTo(ulong killerId)
    {
        if (killerId == OwnerClientId || killerId == 0) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(killerId, out var killerClient))
        {
            if (killerClient.PlayerObject != null && killerClient.PlayerObject.TryGetComponent<PlayerStatsNet>(out var killerStats))
            {
                killerStats.killCount.Value++;
            }
        }
    }

    [ClientRpc]
    private void RespawnOwnerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner)
        {
            RespawnController respawn = FindFirstObjectByType<RespawnController>();
            PlayerController controller = GetComponent<PlayerController>();

            if (respawn != null && controller != null)
                respawn.RespawnPlayer(controller);
            else
                Debug.LogWarning("No se encontro RespawnController o PlayerController local.");
        }
    }
}
