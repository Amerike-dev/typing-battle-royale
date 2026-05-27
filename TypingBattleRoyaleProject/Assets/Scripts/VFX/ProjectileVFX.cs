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
    Transform _target;

    public void Launch(Spell spell, Vector3 direction, float damage = 0f, ulong ownerId = 0, bool isServerCopy = false, Transform target = null)
    {
        _spell = spell;
        _direction = direction.sqrMagnitude > 0f ? direction.normalized : transform.forward;
        _lifeRemaining = spell.speed > 0f ? spell.range / spell.speed : 5f;
        _damage = damage;
        _ownerId = ownerId;
        _isServerCopy = isServerCopy;
        _target = target;
        GetComponent<SpellVFXBinder>().Bind(spell);
    }

    void Update()
    {
        if (_spell == null) return;
        if (_target != null)
        {
            Vector3 toTarget = _target.position - transform.position;

            if (toTarget.sqrMagnitude <= 0.25f)
            {
                SpawnHitVFX();
                Despawn();
                return;
            }
            _direction = toTarget.normalized;
        }
        transform.position += _direction * _spell.speed * Time.deltaTime;
        if (_direction.sqrMagnitude > 0f) transform.rotation = Quaternion.LookRotation(_direction);
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
        _target = null;
        _damage = 0f;
        _ownerId = 0;
        _isServerCopy = false;
        gameObject.SetActive(false);
    }

    void SpawnHitVFX()
    {
        if (_spell == null || _spell.vfxHit == null) return;

        GameObject hit = Instantiate(_spell.vfxHit, transform.position, Quaternion.identity);

        SpellVFXBinder binder = hit.GetComponent<SpellVFXBinder>();

        if (binder != null) binder.Bind(_spell);
    }
}
