using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

public class PlayerStatsNet : NetworkBehaviour
{
    [Header("Config")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private int maxLives = 3;

    [Header("Death / Respawn")]
    [SerializeField] private int respawnSeconds = 3;

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
    public Action<ulong> OnLifeLostWithKiller;
    public Action OnAllLifeLost;
    public Action OnDamageTaken;
    public Action OnEnemyKilled;

    private ulong lastDamageFromClientId;

    private Renderer[] _renderers;
    private CharacterController _characterController;
    private PlayerController _playerController;
    private PlayerAnimatorView _animatorView;
    private Coroutine _deathSequenceCoroutine;

    public PlayerAudio playerAudio;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _characterController = GetComponent<CharacterController>();
        _playerController = GetComponent<PlayerController>();
        _animatorView = GetComponentInChildren<PlayerAnimatorView>(true);
    }

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
        isAlive.OnValueChanged += HandleAliveChanged;
    }

    public override void OnNetworkDespawn()
    {
        currentHP.OnValueChanged -= HandleHPChanged;
        currentLifes.OnValueChanged -= HandleLivesChanged;
        killCount.OnValueChanged -= HandleKillCountChanged;
        isAlive.OnValueChanged -= HandleAliveChanged;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeDamageServerRpc(float damage, ulong attackerId)
    {
        TakeDamage(damage, attackerId);
        
    }

    private void HandleHPChanged(float oldValue, float newValue)
    {
        if (newValue < oldValue) OnDamageTaken?.Invoke();

        // Solo el owner dispara el trigger; el OwnerNetworkAnimator lo replica a los demás.
        // Si la vida llega a 0 dejamos que la animación de muerte tome el control.
        if (IsOwner && newValue < oldValue && newValue > 0f && _animatorView != null)
            _animatorView.TriggerTakeDamage();
    }

    private void HandleAliveChanged(bool oldValue, bool newValue)
    {
        if (oldValue && !newValue && IsOwner && _animatorView != null)
            _animatorView.TriggerDeath();
    }

    private void HandleLivesChanged(int oldValue, int newValue)
    {
        if (newValue < oldValue)
        {
            OnLifeLost?.Invoke();
            OnLifeLostWithKiller?.Invoke(lastDamageFromClientId);
        }

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

        lastDamageFromClientId = attackerId;

        Debug.Log($"[STATS] TakeDamage({damage}) on {ID}");

        currentHP.Value -= damage;
        playerAudio.ChangeSoundById("Dano");

        if (currentHP.Value <= 0) HandleDeath(lastDamageFromClientId);
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
        lastDamageFromClientId = killerId;

        if (killerId != OwnerClientId && killerId != 0) AwardKillTo(killerId);
        if (IsServer) AwardKillTo(killerId);

        if (currentLifes.Value > 1)
        {
            currentLifes.Value--;
            currentHP.Value = 0;
            isAlive.Value = false;

            BeginDeathSequenceOwnerClientRpc(killerId, currentLifes.Value);
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

    [ClientRpc]
    private void BeginDeathSequenceOwnerClientRpc(ulong killerId, int remainingLives, ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;

        if (_deathSequenceCoroutine != null) StopCoroutine(_deathSequenceCoroutine);

        _deathSequenceCoroutine = StartCoroutine(DeathSequenceRoutine(killerId, remainingLives));
    }

    private IEnumerator DeathSequenceRoutine(ulong killerId, int remainingLives)
    {
        SetTemporaryDeathStats(true);

        Debug.Log($"[Death Sequence] KillerId={killerId}, RemainingLives={remainingLives}");

        HUDController hud = FindFirstObjectByType<HUDController>();

        string killerName = GetPlayerNameByClientId(killerId);

        if (hud != null)
        {
            hud.ShowDeathUI(killerName, respawnSeconds, remainingLives);
        }
        else
        {
            Debug.LogWarning("[PlayerStatsNet] No se encontro HUDController para mostrar DeathUI.");
        }

        CameraController cameraController = GetLocalCameraController();

        if (cameraController != null)
        {
            Transform killerTransform = GetPlayerTransformByClientId(killerId);

            if (killerTransform != null)
            {
                cameraController.FollowSpectate(killerTransform);
            }
            else
            {
                Debug.LogWarning($"[PlayerStatsNet] No se encontro Transform del killer con ClientId={killerId}.");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerStatsNet] No se encontro CameraController para seguir al killer.");
        }

        ulong currentSpectatedClientId = killerId;

        for (int secondsLeft = respawnSeconds; secondsLeft > 0; secondsLeft--)
        {
            if (hud != null) hud.UpdateDeathCountdown(secondsLeft);

            currentSpectatedClientId = UpdateSpectateTargetIfNeeded(
                currentSpectatedClientId,
                cameraController
            );

            yield return new WaitForSeconds(1f);
        }

        if (hud != null) hud.UpdateDeathCountdown(0);

        RequestRespawnServerRpc();
    }

    [ServerRpc]
    private void RequestRespawnServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer) return;

        if (currentLifes.Value <= 0)
        {
            Debug.Log("[PlayerStatsNet] No se puede respawnear: no quedan vidas");
            return;
        }

        RespawnController respawnController = FindFirstObjectByType<RespawnController>();

        if (respawnController == null)
        {
            Debug.LogWarning("[PlayerStatsNet] No se encontro RespawnController en la escena.");
            return;
        }

        respawnController.RespawnPlayerServer(this);
    }

    [ServerRpc]
    public void RequestFallRespawnServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer) return;

        if (!isAlive.Value)
        {
            Debug.Log("[PlayerStatsNet] No se respawnea por caida por que el jugador no esta vivo.");
            return;
        }

        RespawnController respawnController = FindFirstObjectByType<RespawnController>();

        if (respawnController == null)
        {
            Debug.Log("[PlayerStatsNet] No se encontro RespawnController para caida.");
            return;
        }

        respawnController.RespawnPlayerFromFallServer(this);
    }

    public void FinishServerRespawn(Vector3 respawnPosition)
    {
        if (!IsServer) return;

        currentHP.Value = maxHP;
        isAlive.Value = true;
        isSpectating.Value = false;

        ForceMoveOwnerClientRpc(respawnPosition);
        FinishRespawnOwnerClientRpc();
    }

    [ClientRpc]
    private void FinishRespawnOwnerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;

        RestoreAfterTemporaryDeath();

        _deathSequenceCoroutine = null;

        Debug.Log("[PlayerStatsNet] Respawn terminando.");
    }

    private string GetPlayerNameByClientId(ulong clientId)
    {
        if (clientId == OwnerClientId)
        {
            return "Yo";
        }

        if (NetworkManager.Singleton == null)
        {
            return $"Jugador {clientId}";
        }

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            if (client.PlayerObject != null)
            {
                PlayerStatsNet stats = client.PlayerObject.GetComponent<PlayerStatsNet>();

                if (stats != null && !string.IsNullOrWhiteSpace(stats.ID))
                {
                    return stats.ID;
                }
            }
        }

        return $"Jugador {clientId}";
    }

    private Transform GetPlayerTransformByClientId(ulong clientId)
    {
        if (NetworkManager.Singleton == null) return null;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            if (client.PlayerObject != null) return client.PlayerObject.transform;
        }

        return null;
    }

    private void SetTemporaryDeathStats(bool isDeadTemporarily)
    {
        if (_renderers != null)
        {
            foreach (Renderer renderer in _renderers)
            {
                if (renderer != null) renderer.enabled = !isDeadTemporarily;
            }
        }

        if (_characterController != null) _characterController.enabled = !isDeadTemporarily;

        if (_playerController != null) _playerController.enabled =!isDeadTemporarily;

        Debug.Log($"[PlayerStatsNet] Temporary death state = {isDeadTemporarily}");
    }

    private void RestoreAfterTemporaryDeath()
    {
        SetTemporaryDeathStats(false);

        HUDController hud = FindFirstObjectByType<HUDController>();

        if (hud != null) hud.HideDeathUI();

        CameraController cameraController = GetLocalCameraController();

        if (cameraController != null)
        {
            cameraController.RestoreLocal();
        }
        else
        {
            Debug.LogWarning("[PlayerStatsNet] No se encontro CameraController.");
        }

        Debug.Log("[PlayerStatsNet] Restaurado despues de su muerte.");
    }

    private bool IsClientAlive(ulong clientId)
    {
        if (NetworkManager.Singleton == null) return false;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            if (client.PlayerObject != null)
            {
                PlayerStatsNet stats = client.PlayerObject.GetComponent<PlayerStatsNet>();

                if (stats != null) return stats.isAlive.Value;
            }
        }

        return false;
    }

    private Transform GetAnotherAlivePlayerTransform(ulong excludedClientId, out ulong foundClientId)
    {
        foundClientId = ulong.MaxValue;

        if (NetworkManager.Singleton == null) return null;

        foreach (var clientPair in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = clientPair.Key;

            if (clientId == OwnerClientId) continue;
            if (clientId == excludedClientId) continue;

            NetworkObject playerObject = clientPair.Value.PlayerObject;

            if (playerObject == null) continue;

            PlayerStatsNet stats = playerObject.GetComponent<PlayerStatsNet>();

            if (stats == null) continue;

            if (stats.isAlive.Value)
            {
                foundClientId = clientId;
                return playerObject.transform;
            }
        }

        return null;
    }

    private ulong UpdateSpectateTargetIfNeeded(ulong currentSpectatedClientId, CameraController cameraController)
    {
        if (cameraController == null) return currentSpectatedClientId;

        if (IsClientAlive(currentSpectatedClientId)) return currentSpectatedClientId;

        ulong newClientId;
        Transform newTarget = GetAnotherAlivePlayerTransform(currentSpectatedClientId, out newClientId);

        if (newTarget != null)
        {
        cameraController.FollowSpectate(newTarget);
        Debug.Log($"[PlayerStatsNet] Killer murió. Cámara cambió a {newTarget.name}.");
        return newClientId;
        }

        Debug.Log("[PlayerStatsNet] Target de spectate murio y no se encontro otro jugador vivo.");
        return currentSpectatedClientId;
    }

    private CameraController GetLocalCameraController()
    {
        CameraController[] cameras = FindObjectsByType<CameraController>(FindObjectsSortMode.None);

        foreach (CameraController cam in cameras)
        {
            if (cam != null && cam.isMine)
            {
                return cam;
            }
        }

        return FindFirstObjectByType<CameraController>();
    }

    [ClientRpc]
    public void ForceMoveOwnerClientRpc(Vector3 targetPosition, ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;

        CharacterController characterController = GetComponent<CharacterController>();

        if (characterController != null) characterController.enabled = false;
        
        transform.position = targetPosition;

        if (characterController != null) characterController.enabled = true;

        Debug.Log("[PlayerStatsNet] Cliente Reposicionado por caida {targetPosition}");
    }


}
