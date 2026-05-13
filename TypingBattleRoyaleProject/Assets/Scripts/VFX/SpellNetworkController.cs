using Unity.Netcode;
using UnityEngine;

public class SpellNetworkController : NetworkBehaviour
{
    [Header("VFX")]
    public GameObject projectileVfxPrefab;
    public Transform castOrigin;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        var caster = GetComponent<CastInputController>();
        if (caster != null) caster.OnSpellCast += HandleLocalSpellCast;
    }

    public override void OnNetworkDespawn()
    {
        var caster = GetComponent<CastInputController>();
        if (caster != null) caster.OnSpellCast -= HandleLocalSpellCast;
    }

    void HandleLocalSpellCast(Spell spell)
    {
        if (!IsOwner || spell == null) return;
        var catalog = SpellCatalog.Instance;
        if (catalog == null)
        {
            Debug.LogWarning("[SpellNetworkController] No SpellCatalog en Resources.");
            return;
        }
        int id = catalog.IndexOf(spell);
        if (id < 0)
        {
            Debug.LogWarning($"[SpellNetworkController] Spell '{spell.spellName}' no esta en el catalogo.");
            return;
        }
        Transform origin = castOrigin != null ? castOrigin : transform;
        CastSpellServerRpc(id, origin.position, origin.forward);
    }

    [ServerRpc]
    void CastSpellServerRpc(int spellId, Vector3 origin, Vector3 direction, ServerRpcParams rpcParams = default)
    {
        PlaySpellVFXClientRpc(spellId, origin, direction, rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    void PlaySpellVFXClientRpc(int spellId, Vector3 origin, Vector3 direction, ulong casterClientId)
    {
        var spell = SpellCatalog.Instance != null ? SpellCatalog.Instance.Get(spellId) : null;
        if (spell == null || projectileVfxPrefab == null) return;

        GameObject go = null;
        if (PoolManager.Instance != null)
            go = PoolManager.Instance.SpawnFromPool("VFX_Projectile", origin, Quaternion.LookRotation(direction));
        if (go == null)
            go = Instantiate(projectileVfxPrefab, origin, Quaternion.LookRotation(direction));

        var projectile = go.GetComponent<ProjectileVFX>();
        if (projectile != null) projectile.Launch(spell, direction);
    }
}
