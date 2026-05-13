using Unity.Netcode;
using UnityEngine;

public class SpellNetworkController : NetworkBehaviour
{
    [Header("VFX prefabs por arquetipo (fallback si PoolManager no tiene el tag)")]
    public GameObject projectileVfxPrefab;
    public GameObject aoeVfxPrefab;
    public GameObject auraVfxPrefab;
    public GameObject beamVfxPrefab;
    public GameObject summonVfxPrefab;
    public GameObject buffDebuffVfxPrefab;

    [Header("Origen del cast")]
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
        if (spell == null) return;

        Transform casterTransform = ResolveCasterTransform(casterClientId);

        switch (spell.archetype)
        {
            case SpellTypes.AOE:
                SpawnAOE(spell, origin);
                break;
            case SpellTypes.Aura:
                SpawnAura(spell, casterTransform);
                break;
            case SpellTypes.Beam:
                SpawnBeam(spell, origin, direction, casterTransform);
                break;
            case SpellTypes.Summon:
                SpawnSummon(spell, origin, direction);
                break;
            case SpellTypes.Buff:
            case SpellTypes.Debuff:
                SpawnBuffDebuff(spell, casterTransform);
                break;
            case SpellTypes.Projectile:
            default:
                SpawnProjectile(spell, origin, direction);
                break;
        }
    }

    GameObject SpawnFromPoolOrInstantiate(string poolTag, GameObject fallbackPrefab, Vector3 pos, Quaternion rot)
    {
        GameObject go = null;
        if (PoolManager.Instance != null)
            go = PoolManager.Instance.SpawnFromPool(poolTag, pos, rot);
        if (go == null && fallbackPrefab != null)
            go = Instantiate(fallbackPrefab, pos, rot);
        if (go == null)
            Debug.LogWarning($"[SpellNetworkController] No hay pool '{poolTag}' ni prefab fallback para el arquetipo.");
        return go;
    }

    void SpawnProjectile(Spell spell, Vector3 origin, Vector3 direction)
    {
        var rot = direction.sqrMagnitude > 0f ? Quaternion.LookRotation(direction) : Quaternion.identity;
        var go = SpawnFromPoolOrInstantiate("VFX_Projectile", projectileVfxPrefab, origin, rot);
        if (go == null) return;
        var vfx = go.GetComponent<ProjectileVFX>();
        if (vfx != null) vfx.Launch(spell, direction);
    }

    void SpawnAOE(Spell spell, Vector3 origin)
    {
        var go = SpawnFromPoolOrInstantiate("VFX_AOE", aoeVfxPrefab, origin, Quaternion.identity);
        if (go == null) return;
        var vfx = go.GetComponent<AOEVFX>();
        if (vfx != null) vfx.Launch(spell, origin);
    }

    void SpawnAura(Spell spell, Transform caster)
    {
        Vector3 pos = caster != null ? caster.position : transform.position;
        var go = SpawnFromPoolOrInstantiate("VFX_Aura", auraVfxPrefab, pos, Quaternion.identity);
        if (go == null) return;
        var vfx = go.GetComponent<AuraVFX>();
        if (vfx != null) vfx.Launch(spell, caster);
    }

    void SpawnBeam(Spell spell, Vector3 origin, Vector3 direction, Transform casterTransform)
    {
        var rot = direction.sqrMagnitude > 0f ? Quaternion.LookRotation(direction) : Quaternion.identity;
        var go = SpawnFromPoolOrInstantiate("VFX_Beam", beamVfxPrefab, origin, rot);
        if (go == null) return;
        var vfx = go.GetComponent<BeamVFX>();
        if (vfx == null) return;
        Transform originTransform = casterTransform != null ? casterTransform : go.transform;
        vfx.Launch(spell, originTransform, direction);
    }

    void SpawnSummon(Spell spell, Vector3 origin, Vector3 direction)
    {
        var rot = direction.sqrMagnitude > 0f ? Quaternion.LookRotation(direction) : Quaternion.identity;
        var go = SpawnFromPoolOrInstantiate("VFX_Summon", summonVfxPrefab, origin, rot);
        if (go == null) return;
        var vfx = go.GetComponent<SummonVFX>();
        if (vfx != null) vfx.Launch(spell, origin, direction);
    }

    void SpawnBuffDebuff(Spell spell, Transform target)
    {
        Vector3 pos = target != null ? target.position : transform.position;
        var go = SpawnFromPoolOrInstantiate("VFX_BuffDebuff", buffDebuffVfxPrefab, pos, Quaternion.identity);
        if (go == null) return;
        var vfx = go.GetComponent<BuffDebuffVFX>();
        if (vfx != null) vfx.Launch(spell, target);
    }

    static Transform ResolveCasterTransform(ulong clientId)
    {
        var nm = NetworkManager.Singleton;
        if (nm == null || nm.SpawnManager == null) return null;
        foreach (var kvp in nm.SpawnManager.SpawnedObjects)
        {
            var no = kvp.Value;
            if (no != null && no.IsPlayerObject && no.OwnerClientId == clientId)
                return no.transform;
        }
        return null;
    }
}
