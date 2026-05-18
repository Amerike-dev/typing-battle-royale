using UnityEngine;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;

public class MonolithView : NetworkBehaviour
{
    [SerializeField] private MonolithData monolithData;
    [SerializeField] private ParticleSystem unlockVFX;
    private Renderer monolithRenderer;
    public NetworkVariable<bool> IsExhausted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone);
    public System.Action<SpellData> OnMonolithUnlocked;

    public int Level => monolithData != null ? monolithData.Level : 0;

    public override void OnNetworkSpawn()
    {
        monolithRenderer = GetComponent<Renderer>();

        Debug.Log($"[MonolithView] OnNetworkSpawn — IsServer:{IsServer} IsClient:{IsClient} | {gameObject.name} | escena:{gameObject.scene.name}");

        IsExhausted.OnValueChanged += OnExhaustedChanged;

        if (IsServer)
            GetComponent<MonolithController>().ServerInitialize();
    }


    public override void OnNetworkDespawn()
    {
        IsExhausted.OnValueChanged -= OnExhaustedChanged;
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
