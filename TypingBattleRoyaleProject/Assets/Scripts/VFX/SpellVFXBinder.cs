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

        // Tamaño y velocidad (con aleatoriedad opcional). El tamaño ya viene reducido en el SO;
        // aquí solo aplicamos el multiplicador por tier y, si se pide, una variación.
        float baseSize = spell.startSize * sizeMul;
        float baseSpeed = spell.startSpeed;
        main.startSize = spell.sizeVariance > 0f
            ? new ParticleSystem.MinMaxCurve(baseSize * (1f - Mathf.Clamp01(spell.sizeVariance)), baseSize)
            : new ParticleSystem.MinMaxCurve(baseSize);
        main.startSpeed = spell.speedVariance > 0f
            ? new ParticleSystem.MinMaxCurve(baseSpeed * (1f - Mathf.Clamp01(spell.speedVariance)), baseSpeed)
            : new ParticleSystem.MinMaxCurve(baseSpeed);
        main.loop = spell.loop;

        // --- Overrides opcionales (neutro = conservar el default del arquetipo) ---
        main.gravityModifier = spell.gravityModifier; // 0 = sin gravedad
        main.simulationSpeed = spell.simulationSpeed > 0f ? spell.simulationSpeed : 1f;
        if (spell.startColorTint.a > 0f) main.startColor = spell.startColorTint; // alpha 0 = sin tinte
        if (spell.startRotationDegrees != 0f) main.startRotation = spell.startRotationDegrees * Mathf.Deg2Rad;
        if (spell.maxParticles > 0) main.maxParticles = spell.maxParticles;

        var emission = _ps.emission;
        emission.rateOverTime = spell.emissionRate * emissionMul;

        var shape = _ps.shape;
        shape.radius = spell.shapeRadius;
        shape.position = spell.emitterOffset; // punto de origen (0,0,0 = sin desplazar)
        if (spell.overrideShape) shape.shapeType = spell.shapeType;

        if (_renderer != null && spell.materialVFX != null)
            _renderer.material = spell.materialVFX;

        _ps.Clear();
        _ps.Play();
    }
}
