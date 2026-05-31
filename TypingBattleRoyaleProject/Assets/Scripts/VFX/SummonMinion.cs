using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Súbdito invocado (p. ej. el Golem de tierra): es un NetworkObject con autoridad de servidor que
/// persigue al enemigo más cercano y lo ataca por ráfagas, hasta que expira su duración.
///
/// Arquitectura:
/// - El SERVIDOR es el único que mueve, busca objetivo y aplica daño (server-authoritative). El
///   movimiento se replica solo a los clientes vía NetworkTransform (agregar ese componente al prefab).
/// - El daño se aplica llamando a PlayerStatsNet.TakeDamage en el servidor, igual que los proyectiles.
/// - Vive 'duration' segundos (del Spell) y luego se despawnea.
///
/// Setup del prefab (ver instrucciones que te dejé): NetworkObject + NetworkTransform (server auth) +
/// una malla (esfera) + este script. Registrar el prefab en el NetworkManager (NetworkPrefabs) y
/// asignarlo a SpellNetworkController.summonMinionPrefab.
/// </summary>
public class SummonMinion : NetworkBehaviour
{
    [Header("Combate (se sobreescriben desde el Spell al invocar)")]
    [Tooltip("Daño por golpe.")]
    [SerializeField] private float damagePerHit = 10f;
    [Tooltip("Golpes por ráfaga.")]
    [SerializeField] private int hitsPerBurst = 3;
    [Tooltip("Segundos entre ráfagas.")]
    [SerializeField] private float burstInterval = 4f;
    [Tooltip("Segundos entre los golpes dentro de una misma ráfaga.")]
    [SerializeField] private float timeBetweenHits = 0.35f;

    [Header("Movimiento")]
    [Tooltip("Velocidad de persecución (lento, como una mole de piedra).")]
    [SerializeField] private float moveSpeed = 2.5f;
    [Tooltip("Distancia a la que se considera 'en rango' para golpear.")]
    [SerializeField] private float attackRange = 2.2f;
    [Tooltip("Altura fija sobre el suelo donde flota/rueda la esfera (0 = usa la Y de spawn).")]
    [SerializeField] private float groundY = 0f;

    private ulong _ownerClientId;
    private float _despawnAt;
    private float _nextBurstAt;
    private Transform _currentTarget;

    /// <summary>Llamado en el SERVIDOR justo después de Spawn() para configurar el súbdito desde el Spell.</summary>
    public void ServerInit(Spell spell, ulong ownerClientId)
    {
        _ownerClientId = ownerClientId;
        float duration = spell != null && spell.duration > 0f ? spell.duration : 10f;
        _despawnAt = Time.time + duration;
        _nextBurstAt = Time.time + burstInterval;

        if (spell != null && spell.damage > 0f)
        {
            // Daño por golpe = damage del Spell (se tunea directo en el .asset del Golem).
            // Recomendación de balance: que no supere el doble de "tira piedra" (15) -> <= 30 por golpe.
            damagePerHit = spell.damage;
        }

        if (groundY == 0f) groundY = transform.position.y;
    }

    private void Update()
    {
        if (!IsServer) return;

        if (Time.time >= _despawnAt)
        {
            if (NetworkObject != null && NetworkObject.IsSpawned) NetworkObject.Despawn(true);
            return;
        }

        _currentTarget = FindClosestEnemy();
        if (_currentTarget == null) return;

        // Persigue manteniendo la altura.
        Vector3 to = _currentTarget.position - transform.position;
        to.y = 0f;
        float dist = to.magnitude;

        if (dist > attackRange)
        {
            Vector3 step = to.normalized * moveSpeed * Time.deltaTime;
            Vector3 next = transform.position + step;
            next.y = groundY;
            transform.position = next;
            // Mira hacia el objetivo (horizontal).
            if (to.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(to.normalized, Vector3.up);
        }
        else if (Time.time >= _nextBurstAt)
        {
            StartCoroutine(AttackBurst(_currentTarget));
            _nextBurstAt = Time.time + burstInterval;
        }
    }

    private System.Collections.IEnumerator AttackBurst(Transform target)
    {
        for (int i = 0; i < hitsPerBurst; i++)
        {
            if (target == null) yield break;
            var stats = target.GetComponentInParent<PlayerStatsNet>();
            if (stats != null && stats.isAlive.Value)
                stats.TakeDamage(damagePerHit, _ownerClientId);
            yield return new WaitForSeconds(timeBetweenHits);
        }
    }

    /// <summary>Enemigo vivo más cercano que no sea el dueño del súbdito (server-side).</summary>
    private Transform FindClosestEnemy()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null) return null;

        Transform best = null;
        float bestSqr = float.MaxValue;

        foreach (var kvp in nm.ConnectedClients)
        {
            if (kvp.Key == _ownerClientId) continue;
            var po = kvp.Value != null ? kvp.Value.PlayerObject : null;
            if (po == null) continue;
            var stats = po.GetComponent<PlayerStatsNet>();
            if (stats == null || !stats.isAlive.Value) continue;

            float sqr = (po.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = po.transform; }
        }
        return best;
    }
}
