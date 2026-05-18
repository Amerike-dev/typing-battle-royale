using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SpellVFXBinder : MonoBehaviour
{
    static readonly float[] SizeMul = { 1f, 1.4f, 2f };
    static readonly float[] EmissionMul = { 1f, 1.5f, 2.5f };

    ParticleSystem _ps;
    ParticleSystemRenderer _renderer;

    void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
        _renderer = GetComponent<ParticleSystemRenderer>();
    }

    public void Bind(Spell spell)
    {
        if (spell == null) return;
        if (_ps == null) _ps = GetComponent<ParticleSystem>();
        if (_renderer == null) _renderer = GetComponent<ParticleSystemRenderer>();

        int t = Mathf.Clamp((int)spell.tier, 0, SizeMul.Length - 1);
        float sizeMul = SizeMul[t];
        float emissionMul = EmissionMul[t];

        var main = _ps.main;
        if (spell.particleLifeDuration > 0f) main.startLifetime = spell.particleLifeDuration;
        main.startSpeed = spell.startSpeed;
        main.startSize = spell.startSize * sizeMul;
        main.loop = spell.loop;

        var emission = _ps.emission;
        emission.rateOverTime = spell.emissionRate * emissionMul;

        var shape = _ps.shape;
        shape.radius = spell.shapeRadius;

        if (_renderer != null && spell.materialVFX != null)
            _renderer.material = spell.materialVFX;

        _ps.Clear();
        _ps.Play();
    }
}
