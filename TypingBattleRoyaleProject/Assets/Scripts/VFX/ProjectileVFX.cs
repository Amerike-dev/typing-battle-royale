using UnityEngine;

[RequireComponent(typeof(SpellVFXBinder))]
public class ProjectileVFX : MonoBehaviour
{
    [Tooltip("Segundos de 'armado' tras nacer: durante ese lapso el proyectil ignora colisiones, para no autodestruirse dentro del collider del caster o del suelo al spawnear.")]
    [SerializeField] float armingSeconds = 0.08f;

    Spell _spell;
    Vector3 _direction;
    float _lifeRemaining;
    bool _isServerCopy;
    ulong _ownerId;
    float _damage;
    Transform _target;
    Transform _casterRoot;
    float _armUntil;

    public void Launch(Spell spell, Vector3 direction, float damage = 0f, ulong ownerId = 0, bool isServerCopy = false, Transform target = null, Transform casterRoot = null)
    {
        _spell = spell;
        _direction = direction.sqrMagnitude > 0f ? direction.normalized : transform.forward;
        _lifeRemaining = spell.speed > 0f ? spell.range / spell.speed : 5f;
        _damage = damage;
        _ownerId = ownerId;
        _isServerCopy = isServerCopy;
        _target = target;
        _casterRoot = casterRoot;
        _armUntil = Time.time + Mathf.Max(0f, armingSeconds);
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
        if (_spell == null) return;             // ya despawneado
        if (Time.time < _armUntil) return;       // armado: ignora el spawn dentro del caster/suelo
        if (IsCaster(other)) return;             // nunca chocar con quien lo lanzó (por jerarquía)

        var otherStats = other.GetComponent<PlayerStatsNet>();
        if (otherStats == null) otherStats = other.GetComponentInParent<PlayerStatsNet>();

        if (otherStats != null && otherStats.OwnerClientId == _ownerId) return; // respaldo por dueño

        if (_isServerCopy && otherStats != null)
        {
            if (_damage > 0f) otherStats.TakeDamage(_damage, _ownerId);

            // Efecto de estado al impactar (Slow/Freeze/Root/Poison), si el hechizo lo define.
            if (_spell.debuff != StatusEffects.None && _spell.statusDuration > 0f)
                otherStats.ApplyStatusServer(_spell.debuff, _spell.statusMagnitude, _spell.statusDuration, _ownerId);
        }

        SpawnHitVFX();
        Despawn();
    }

    /// <summary>True si el collider pertenece al caster (él mismo o cualquier hijo de su jerarquía).</summary>
    bool IsCaster(Collider other)
    {
        if (_casterRoot == null || other == null) return false;
        Transform t = other.transform;
        return t == _casterRoot || t.IsChildOf(_casterRoot);
    }

    void Despawn()
    {
        _spell = null;
        _target = null;
        _casterRoot = null;
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
