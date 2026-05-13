using UnityEngine;

[RequireComponent(typeof(SpellVFXBinder))]
public class ProjectileVFX : MonoBehaviour
{
    Spell _spell;
    Vector3 _direction;
    float _lifeRemaining;

    public void Launch(Spell spell, Vector3 direction)
    {
        _spell = spell;
        _direction = direction.sqrMagnitude > 0f ? direction.normalized : transform.forward;
        _lifeRemaining = spell.speed > 0f ? spell.range / spell.speed : 5f;
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
        Despawn();
    }

    void Despawn()
    {
        _spell = null;
        gameObject.SetActive(false);
    }
}
