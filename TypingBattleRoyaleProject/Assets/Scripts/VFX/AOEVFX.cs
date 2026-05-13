using UnityEngine;

[RequireComponent(typeof(SpellVFXBinder))]
public class AOEVFX : MonoBehaviour
{
    Spell _spell;
    float _lifeRemaining;
    float _initialRadius;

    public void Launch(Spell spell, Vector3 origin)
    {
        _spell = spell;
        transform.position = origin;
        transform.rotation = Quaternion.identity;
        _initialRadius = spell.range > 0f ? spell.range : 5f;
        transform.localScale = Vector3.one * _initialRadius;
        _lifeRemaining = spell.duration > 0f ? spell.duration : 1.5f;
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
        gameObject.SetActive(false);
    }
}
