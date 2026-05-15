using UnityEngine;

[RequireComponent(typeof(SpellVFXBinder))]
public class ProjectileVFX : MonoBehaviour
{
    Spell _spell;
    Vector3 _direction;
    float _lifeRemaining;
    bool _isServerCopy;
    ulong _ownerId;
    float _damage;

    public void Launch(Spell spell, Vector3 direction, float damage = 0f, ulong ownerId = 0, bool isServerCopy = false)
    {
        _spell = spell;
        _direction = direction.sqrMagnitude > 0f ? direction.normalized : transform.forward;
        _lifeRemaining = spell.speed > 0f ? spell.range / spell.speed : 5f;
        _damage = damage;
        _ownerId = ownerId;
        _isServerCopy = isServerCopy;
        GetComponent<SpellVFXBinder>().Bind(spell);
    }

    void Update()
    {
        if (_spell == null) return;
        transform.position += _direction * _spell.speed * Time.deltaTime;
        _lifeRemaining -= Time.deltaTime;
        if (_lifeRemaining <= 0f) Despawn();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_isServerCopy && _damage > 0f)
        {
            var targetStats = other.GetComponent<PlayerStatsNet>();
            if (targetStats != null && targetStats.OwnerClientId != _ownerId)
            {
                targetStats.TakeDamage(_damage, _ownerId);
            }
        }
        Despawn();
    }

    void Despawn()
    {
        _spell = null;
        gameObject.SetActive(false);
    }
}
