using UnityEngine;

[RequireComponent(typeof(SpellVFXBinder))]
public class AuraVFX : MonoBehaviour
{
    Spell _spell;
    Transform _caster;
    float _lifeRemaining;

    public void Launch(Spell spell, Transform caster)
    {
        _spell = spell;
        _caster = caster;
        if (caster != null)
        {
            transform.SetParent(caster, worldPositionStays: false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        _lifeRemaining = spell.duration > 0f ? spell.duration : 3f;
        GetComponent<SpellVFXBinder>().Bind(spell);
    }

    void Update()
    {
        if (_spell == null) return;
        _lifeRemaining -= Time.deltaTime;
        if (_lifeRemaining <= 0f) Despawn();
    }

    void Despawn()
    {
        _spell = null;
        _caster = null;
        if (transform.parent != null) transform.SetParent(null, worldPositionStays: true);
        gameObject.SetActive(false);
    }
}
