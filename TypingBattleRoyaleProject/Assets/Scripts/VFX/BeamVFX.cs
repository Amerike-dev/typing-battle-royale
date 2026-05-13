using UnityEngine;

[RequireComponent(typeof(SpellVFXBinder))]
[RequireComponent(typeof(LineRenderer))]
public class BeamVFX : MonoBehaviour
{
    Spell _spell;
    Transform _origin;
    Transform _target;
    Vector3 _fallbackTargetPos;
    bool _useFallbackTarget;
    LineRenderer _line;
    float _lifeRemaining;

    void Awake()
    {
        _line = GetComponent<LineRenderer>();
    }

    public void Launch(Spell spell, Transform origin, Transform target)
    {
        _spell = spell;
        _origin = origin;
        _target = target;
        _useFallbackTarget = false;
        _lifeRemaining = spell.duration > 0f ? spell.duration : 1.5f;
        if (_line != null)
        {
            _line.enabled = true;
            if (spell.materialVFX != null) _line.material = spell.materialVFX;
        }
        GetComponent<SpellVFXBinder>().Bind(spell);
    }

    public void Launch(Spell spell, Transform origin, Vector3 direction)
    {
        _spell = spell;
        _origin = origin;
        _target = null;
        float reach = spell.range > 0f ? spell.range : 10f;
        Vector3 dir = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
        Vector3 start = origin != null ? origin.position : transform.position;
        _fallbackTargetPos = start + dir * reach;
        _useFallbackTarget = true;
        _lifeRemaining = spell.duration > 0f ? spell.duration : 1.5f;
        if (_line != null)
        {
            _line.enabled = true;
            if (spell.materialVFX != null) _line.material = spell.materialVFX;
        }
        GetComponent<SpellVFXBinder>().Bind(spell);
    }

    void Update()
    {
        if (_spell == null) return;
        if (_line != null && _origin != null)
        {
            _line.SetPosition(0, _origin.position);
            Vector3 endPos = _useFallbackTarget || _target == null ? _fallbackTargetPos : _target.position;
            _line.SetPosition(1, endPos);
        }
        _lifeRemaining -= Time.deltaTime;
        if (_lifeRemaining <= 0f) Despawn();
    }

    void Despawn()
    {
        _spell = null;
        _origin = null;
        _target = null;
        if (_line != null) _line.enabled = false;
        gameObject.SetActive(false);
    }
}
