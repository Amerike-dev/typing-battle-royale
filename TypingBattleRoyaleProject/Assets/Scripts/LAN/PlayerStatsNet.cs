using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Collections;
using System.Collections.Generic;

public class PlayerStatsNet : NetworkBehaviour
{
    [Header("Config")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private int maxLives = 3;

    public float MaxHP => maxHP;
    public int MaxLives => maxLives;

    public NetworkVariable<float> currentHP = new(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> currentLifes = new(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> killCount = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isAlive = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isSpectating = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> wPM = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<FixedString32Bytes> networkPlayerID = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Game stats")]
    public NetworkVariable<float> damageDealt = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> damageTaken = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> spellsCast = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public float avgWpm;
    public float avgAccuracy;
    public float fastestCastSeconds;
    public Dictionary<string, int> spellUsageCount = new Dictionary<string, int>();

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
            isSpectating.Value = false;

            damageDealt.Value = 0f;
            damageTaken.Value = 0f;
            spellsCast.Value = 0;
        }

        if (IsOwner)
        {
            avgWpm = 0f;
            avgAccuracy = 0f;
            fastestCastSeconds = float.MaxValue;
            spellUsageCount.Clear();
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

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeDamageServerRpc(float damage, ulong attackerId)
    {
        TakeDamage(damage, attackerId);
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
            if (IsServer)
            {
                isAlive.Value = false;
                isSpectating.Value = true;
            }

            OnAllLifeLost?.Invoke();
        }
    }

    private void HandleKillCountChanged(int oldValue, int newValue)
    {
        if (newValue > oldValue) OnEnemyKilled?.Invoke();
    }

    public void TakeDamage(float damage, ulong attackerId = ulong.MaxValue)
    {
        if (!IsServer || !isAlive.Value) return;

        damageTaken.Value += damage;
        currentHP.Value -= damage;

        if (attackerId != ulong.MaxValue && attackerId != OwnerClientId && NetworkManager.Singleton.ConnectedClients.TryGetValue(attackerId, out var attackerClient))
        {
            if (attackerClient.PlayerObject != null && attackerClient.PlayerObject.TryGetComponent<PlayerStatsNet>(out var attackerStats))
            {
                attackerStats.damageDealt.Value += damage;
            }
        }

        if (currentHP.Value <= 0) HandleDeath(attackerId);
    }

    public void RegisterLocalSpellCast(string spellName, float castTime, float accuracy, float currentWPM)
    {
        if (!IsOwner) return;

        if (spellUsageCount.ContainsKey(spellName)) spellUsageCount[spellName]++;
        else spellUsageCount[spellName] = 1;

        if (castTime < fastestCastSeconds) fastestCastSeconds = castTime;

        avgWpm = avgWpm == 0f ? currentWPM : (avgWpm + currentWPM) / 2f;
        avgAccuracy = avgAccuracy == 0f ? accuracy : (avgAccuracy + accuracy) / 2f;

        SubmitSpellCastServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void SubmitSpellCastServerRpc()
    {
        spellsCast.Value++;
    }

    private void HandleDeath(ulong killerId)
    {
        if (IsServer) AwardKillTo(killerId);

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
            isSpectating.Value = true;

            EnterSpectatorModeClientRpc();
        }
    }

    private void AwardKillTo(ulong killerId)
    {
        if (killerId == OwnerClientId || killerId == ulong.MaxValue) return;

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

    [ClientRpc]
    private void EnterSpectatorModeClientRpc(ClientRpcParams clientRpcParams = default)
    {

        PlayerController controller = GetComponent<PlayerController>();

        if (controller != null)
        {
            controller.EnterSpectatorMode();
        }
        else
        {
            Debug.LogWarning("[PlayerController] No se encontro PlayerController local para entrar en modo espectador.");
        }

    }
}
