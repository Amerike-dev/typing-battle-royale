using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

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

    private CastInputController _caster;
    private PlayerAnimatorView _animatorView;
    private TargetSystem _targetSystem;
    private readonly Dictionary<int, float> _lastCastTimes = new();
    private static readonly Dictionary<ulong, Dictionary<int, float>> _serverLastCastTimes = new();

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        _caster = GetComponent<CastInputController>();
        _animatorView = GetComponent<PlayerAnimatorView>();
        _targetSystem = GetComponent<TargetSystem>();
        Debug.Log($"[TBR-004][SPAWN] TargetSystem encontrado={_targetSystem != null}");
        if (_caster != null) _caster.OnSpellCast += HandleLocalSpellCast;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        if (_caster != null) _caster.OnSpellCast -= HandleLocalSpellCast;
    }

    void HandleLocalSpellCast(Spell spell)
    {
        if (!IsOwner || spell == null || _caster == null) return;

        Debug.Log($"[SpellNetworkController] HandleLocalSpellCast recibio spell: {spell.spellName}");

        var catalog = SpellCatalog.Instance;
        if (catalog == null)
        {
            Debug.LogWarning("[SpellNetworkController] No se encontró SpellCatalog en Resources.");
            return;
        }
        int id = catalog.IndexOf(spell);
        if (id < 0)
        {
            Debug.LogWarning($"[SpellNetworkController] El hechizo '{spell.spellName}' no está en el catálogo.");
            return;
        }

        if (_lastCastTimes.TryGetValue(id, out float lastCastTime))
        {
            if (Time.time < lastCastTime + spell.cooldown) // Ahora 'cooldown' existe en Spell.cs
            {
                Debug.Log($"[SpellNetworkController] Hechizo '{spell.spellName}' en cooldown.");
                return;
            }
        }
        _lastCastTimes[id] = Time.time;

        Debug.Log($"[TBR-004][LOCAL] Cooldown local aprobado. spellId={id}, spell={spell.spellName}");

        if (_animatorView != null)
        {
            Debug.Log("[TBR-004][LOCAL] Disparando animacion Casting.");
            _animatorView.TriggerCasting();
        }
        else
        {
            Debug.LogWarning("[TBR-004][LOCAL] No hay PlayerAnimatorView.");
        }

        Transform origin = castOrigin != null ? castOrigin : transform;

        ulong targetNetworkObjectId = 0;
        bool hasTarget = false;

        Transform currentTarget = GetCurrentTarget();

        Debug.Log($"[TBR-004][TARGET CHECK] TargetSystem existe={_targetSystem != null}");

        if (currentTarget != null)
        {
            NetworkObject targetNetObj = currentTarget.GetComponent<NetworkObject>();

            if (targetNetObj == null) targetNetObj = currentTarget.GetComponentInParent<NetworkObject>();

            Debug.Log($"[TBR-004][TARGET CHECK] Target tiene NetworkObject={targetNetObj != null}");

            if (targetNetObj != null)
            {
                targetNetworkObjectId = targetNetObj.NetworkObjectId;
                hasTarget = true;

                Debug.Log($"[TBR-004][TARGET CHECK] Target Ok. targetNetworkObjectId={targetNetworkObjectId}");
            }
        }
        else
        {
            Debug.Log($"[TBR-004][TARGET CHECK] No hay target valido para mandar al ServerRpc.");
        }

        Debug.Log($"[TBR-004][LOCAL] Enviando CastSpellServerRpc. spellId={id}, hasTarget={hasTarget}, targetId={targetNetworkObjectId}, accuracy={_caster.accuracy}");

        // La precisión se obtiene del CastInputController, que la calcula antes de disparar el evento.
        CastSpellServerRpc(id, origin.position, origin.forward, _caster.accuracy, targetNetworkObjectId, hasTarget);
    }

    [ServerRpc]
    void CastSpellServerRpc(int spellId, Vector3 origin, Vector3 direction, float accuracy, ulong targetNetworkObjectId, bool hasTarget, ServerRpcParams rpcParams = default)
    {
        var spell = SpellCatalog.Instance.Get(spellId);
        if (spell == null) return;

        ulong casterClientId = rpcParams.Receive.SenderClientId;

        Debug.Log($"[TBR-004][SERVER] Recibi cast. caster={casterClientId}, spellId={spellId}, spell={spell.spellName}, accurancy={accuracy}, hasTarget={hasTarget}");

        if (!CanCastServer(casterClientId, spellId, spell.cooldown))
        {
            Debug.Log($"[SERVER] Cliente {casterClientId} intento castear '{spell.spellName}' en cooldown.");
            return;
        }

        RegisterCastServer(casterClientId, spellId);

        float damageMultiplier = accuracy < 30f
            ? 0f
            : TypingStats.GetDamageBonusMultiplier(accuracy);
        
        float finalDamage = spell.damage * damageMultiplier;

        Debug.Log($"[SERVER] Spells='{spell.spellName}', Accuracy={accuracy}, Multiplier={damageMultiplier}, Damage={finalDamage}");

        PlaySpellVFXClientRpc(spellId, origin, direction, casterClientId, finalDamage, targetNetworkObjectId, hasTarget);
    }

    [ClientRpc]
    void PlaySpellVFXClientRpc(int spellId, Vector3 origin, Vector3 direction, ulong casterClientId, float damage, ulong targetNetworkObjectId, bool hasTarget)
    {
        var spell = SpellCatalog.Instance != null ? SpellCatalog.Instance.Get(spellId) : null;
        if (spell == null) return;

        Debug.Log($"[TBR-004][CLIENT VFX] Reproduciendo VFX. spell={spell.spellName}, caster={casterClientId}, hasTarget={hasTarget}, targetId={targetNetworkObjectId}, damage={damage}");

        Transform casterTransform = ResolveCasterTransform(casterClientId);
        Transform targetTransform = hasTarget ? ResolveNetworkObjectTransform(targetNetworkObjectId) : null;

        SpawnCastVFX(spell, origin, casterTransform);

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
                SpawnProjectile(spell, origin, direction, damage, casterClientId, targetTransform);
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

    void SpawnProjectile(Spell spell, Vector3 origin, Vector3 direction, float damage, ulong casterClientId, Transform targetTransform)
    {
        var rot = direction.sqrMagnitude > 0f ? Quaternion.LookRotation(direction) : Quaternion.identity;
        GameObject go = null;
        if (spell.vfxProjectile != null)
        {
            go =Instantiate(spell.vfxProjectile, origin, rot);
        }
        else
        {
            go = SpawnFromPoolOrInstantiate("VFX_Projectile", projectileVfxPrefab, origin, rot);
        }
        if (go == null) return;
        var vfx = go.GetComponent<ProjectileVFX>(); // El daño se calcula en el servidor y se pasa aquí
        if (vfx != null) vfx.Launch(spell, direction, damage, casterClientId, IsServer, targetTransform); // IsServer asegura que el daño solo se aplique en el servidor
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

    static Transform ResolveNetworkObjectTransform(ulong NetworkObjectId)
    {
        var nm = NetworkManager.Singleton;

        if (nm == null || nm.SpawnManager == null) return null;

        if(nm.SpawnManager.SpawnedObjects.TryGetValue(NetworkObjectId, out NetworkObject netObj)) return netObj != null ? netObj.transform : null;

        return null;
    }

    void SpawnCastVFX(Spell spell, Vector3 Origin, Transform casterTransform)
    {
        if (spell == null || spell.vfxCast == null) return;

        Vector3 spawnPos = Origin;
        Quaternion spawnRot = Quaternion.identity;

        if (casterTransform != null)
        {
            SpellNetworkController casterSpellController = casterTransform.GetComponent<SpellNetworkController>();

            if (casterSpellController != null && casterSpellController.castOrigin != null)
            {
                spawnPos = casterSpellController.castOrigin.position;
                spawnRot = casterSpellController.castOrigin.rotation;
            }
            else
            {
                spawnPos = casterTransform.position + casterTransform.forward * 0.8f + Vector3.up * 1.2f;
                spawnRot = casterTransform.rotation;
            }
        }

        GameObject go = Instantiate(spell.vfxCast, spawnPos, spawnRot);

        SpellVFXBinder binder = go.GetComponent<SpellVFXBinder>();

        if (binder != null) binder.Bind(spell);
    }

    private Transform GetCurrentTarget()
    {
        if (_targetSystem != null && _targetSystem.CurrentTarget != null)
        {
            Debug.Log($"[TBR-004][TARGET SOURCE] Target desde _targetSystem local: {_targetSystem.CurrentTarget.name}");
            return _targetSystem.CurrentTarget;
        }

        if (GameplayManager.Instance != null && GameplayManager.Instance.TargetSystem != null && GameplayManager.Instance.TargetSystem.CurrentTarget != null)
        {
            Debug.Log($"[TBR-004][TARGET SOURCE] Target desde GameplayManager.TargetSystem: {GameplayManager.Instance.TargetSystem.CurrentTarget.name}");
            return GameplayManager.Instance.TargetSystem.CurrentTarget;
        }

        Debug.LogWarning("[TBR-004][TARGET SOURCE] No se encontro Currenttarget en ningun lugar.");
        return null;
    }

    private bool CanCastServer(ulong casterClientId, int  spellId, float cooldown)
    {
        if (!_serverLastCastTimes.TryGetValue(casterClientId, out Dictionary<int, float> playerCooldowns)) return true;

        if (!playerCooldowns.TryGetValue(spellId, out float lastCastTime)) return true;

        return Time.time >= lastCastTime + cooldown;
    }

    private void RegisterCastServer(ulong casterClientId, int spellId)
    {
        if (!_serverLastCastTimes.TryGetValue(casterClientId, out Dictionary<int, float> playerCooldowns))
        {
            playerCooldowns = new Dictionary<int, float>();
            _serverLastCastTimes[casterClientId] = playerCooldowns;
        }

        playerCooldowns[spellId] = Time.time;
    }
}
