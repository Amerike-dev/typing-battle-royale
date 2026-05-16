using UnityEngine;
using Unity.Netcode;

public class MonolithView : NetworkBehaviour
{
    [SerializeField] private MonolithData monolithData;
    [SerializeField] private ParticleSystem unlockVFX;
    [SerializeField] private Renderer monolithRenderer;
    public NetworkVariable<bool> IsExhausted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone);
    public System.Action<SpellData> OnMonolithUnlocked;

    public static event System.Action<MonolithView> OnMonolithRegistered;
    public static event System.Action<MonolithView> OnMonolithUnregistered;

    public int Level => monolithData != null ? monolithData.Level : 0;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[MonolithView] OnNetworkSpawn — IsServer:{IsServer} IsClient:{IsClient} IsHost:{IsHost} | {gameObject.name}");

        IsExhausted.OnValueChanged += OnExhaustedChanged;
        OnMonolithRegistered?.Invoke(this);
    }

    public override void OnNetworkDespawn()
    {
        IsExhausted.OnValueChanged -= OnExhaustedChanged;
        OnMonolithUnregistered?.Invoke(this);
    }

    private void OnExhaustedChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            PlayUnlockVFX();
        }
    }

    public bool TryInteract(PlayerStats interactingPlayer)
    {
        if (IsExhausted.Value) { return false; }
        ;
        InteractRpc();
        return true;
    }

    [Rpc(SendTo.Server)]
    private void InteractRpc(RpcParams rpcParams = default)
    {
        if (IsExhausted.Value) return;

        var clientId = rpcParams.Receive.SenderClientId;
        var controller = GetComponent<MonolithController>();

        if (controller.IdPlayerExist(clientId.ToString())) return;
        if (monolithData == null || monolithData.spellData == null) return;

        IsExhausted.Value = true;
    }

    public void PlayUnlockVFX()
    {
        if (unlockVFX == null) return;
        unlockVFX.Play();
    }
}
