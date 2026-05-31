using UnityEngine;

[RequireComponent(typeof(SpellVFXBinder))]
public class SummonVFX : MonoBehaviour
{
    [Header("Surgir del piso (p. ej. Montaña)")]
    [Tooltip("Si está activo, la invocación emerge desde abajo del suelo hasta su posición final.")]
    [SerializeField] bool riseFromGround = false;
    [Tooltip("Distancia que sube desde abajo del suelo hasta su posición final.")]
    [SerializeField] float riseDistance = 4f;
    [Tooltip("Segundos que tarda en emerger.")]
    [SerializeField] float riseDuration = 0.8f;

    Spell _spell;
    float _lifeRemaining;
    Vector3 _finalPos;
    float _riseElapsed;

    public void Launch(Spell spell, Vector3 position, Vector3 direction)
    {
        _spell = spell;
        _finalPos = position;
        if (direction.sqrMagnitude > 0f)
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        else
            transform.rotation = Quaternion.identity;

        // Si emerge del piso, arranca hundido y sube en Update.
        _riseElapsed = 0f;
        transform.position = riseFromGround ? position + Vector3.down * riseDistance : position;

        _lifeRemaining = spell.duration > 0f ? spell.duration : 5f;
        GetComponent<SpellVFXBinder>().Bind(spell);
    }

    void Update()
    {
        if (_spell == null) return;

        if (riseFromGround && _riseElapsed < riseDuration)
        {
            _riseElapsed += Time.deltaTime;
            float k = Mathf.Clamp01(_riseElapsed / Mathf.Max(0.0001f, riseDuration));
            // Ease-out para un golpe seco al final.
            float eased = 1f - (1f - k) * (1f - k);
            transform.position = Vector3.Lerp(_finalPos + Vector3.down * riseDistance, _finalPos, eased);
        }

        _lifeRemaining -= Time.deltaTime;
        if (_lifeRemaining <= 0f) Despawn();
    }

    void Despawn()
    {
        _spell = null;
        gameObject.SetActive(false);
    }
}
