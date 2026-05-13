using UnityEngine;

[RequireComponent(typeof(SpellVFXBinder))]
public class SummonVFX : MonoBehaviour
{
    Spell _spell;
    float _lifeRemaining;

    public void Launch(Spell spell, Vector3 position, Vector3 direction)
    {
        _spell = spell;
        transform.position = position;
        if (direction.sqrMagnitude > 0f)
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        else
            transform.rotation = Quaternion.identity;
        _lifeRemaining = spell.duration > 0f ? spell.duration : 5f;
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
