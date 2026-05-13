using UnityEngine;

[RequireComponent(typeof(SpellVFXBinder))]
public class BuffDebuffVFX : MonoBehaviour
{
    [SerializeField] Vector3 floatOffset = new Vector3(0f, 1.8f, 0f);

    Spell _spell;
    Transform _target;
    float _lifeRemaining;

    public void Launch(Spell spell, Transform target)
    {
        _spell = spell;
        _target = target;
        _lifeRemaining = spell.duration > 0f ? spell.duration : 3f;
        if (target != null) transform.position = target.position + floatOffset;
        GetComponent<SpellVFXBinder>().Bind(spell);
    }

    void LateUpdate()
    {
        if (_spell == null || _target == null) return;
        transform.position = _target.position + floatOffset;
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
        _target = null;
        gameObject.SetActive(false);
    }
}
