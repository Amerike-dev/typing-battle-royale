using UnityEngine;

public enum Elements { None = 0, Fire = 1, Water = 2, Earth = 3, Wind = 4, Nature = 5, Thunder = 6, Dark, Light, Ice, Lava}
public enum SpellTiers { T1, T2, T3 }
public enum SpellTypes {Projectile, Movility, Summon, Buff, Debuff, Aura, AOE, Weapon, Beam};
public enum StatusEffects {None, Slow, Freeze, Root, Poison}
[CreateAssetMenu(fileName = "Spell", menuName = "Scriptable Objects/Spell")]

public class Spell : ScriptableObject
{
    [Header("General Info")]
    public string spellName;
    public string runeString;
    public Elements elementType;
    public SpellTiers tier;
    public SpellTypes archetype;
    public string description;
    [Header("Characteristics")]
    public SpellTypes[] spellTypes;
    public float damage;
    [Tooltip("Tiempo en segundos que debe esperar el jugador antes de poder volver a lanzar este hechizo.")]
    public float cooldown;
    public StatusEffects debuff;
    public float range;
    public float speed;
    public int uses;
    public float duration;
    [Header("VFX Prefabs")]
    public GameObject vfxCast;
    public GameObject vfxProjectile;
    public GameObject vfxHit;
    [Header("SFX")]
    public float particleLifeDuration;
    public bool loop = true;
    public float startSpeed=2f;
    public float startSize=1f;
    public int emissionRate = 50;
    public float shapeRadius = 2.0f;
    public Material materialVFX;

    [Header("VFX Tuning (overrides opcionales; el valor neutro = usar el default del arquetipo)")]
    [Tooltip("Punto de origen de la emisión, relativo al sistema de partículas (shape.position). 0,0,0 = sin desplazar.")]
    public Vector3 emitterOffset = Vector3.zero;
    [Tooltip("Gravedad aplicada a las partículas (main.gravityModifier). 0 = sin gravedad.")]
    public float gravityModifier = 0f;
    [Tooltip("Velocidad global de reproducción del efecto (main.simulationSpeed). <=0 = usar 1 (normal); 0.5 = mitad de rápido.")]
    public float simulationSpeed = 1f;
    [Tooltip("Tinte de color de las partículas (multiplica al material). Alpha 0 = sin tinte.")]
    public Color startColorTint = Color.white;
    [Tooltip("Rotación inicial de cada partícula en grados (main.startRotation). 0 = ninguna.")]
    public float startRotationDegrees = 0f;
    [Tooltip("Aleatoriedad del tamaño 0..1: 0 = tamaño constante; 0.3 = entre 70% y 100% del tamaño.")]
    [Range(0f, 1f)] public float sizeVariance = 0f;
    [Tooltip("Aleatoriedad de la velocidad inicial 0..1: 0 = constante.")]
    [Range(0f, 1f)] public float speedVariance = 0f;
    [Tooltip("Si está activo, fuerza la forma del emisor a 'shapeType'; si no, conserva la del arquetipo.")]
    public bool overrideShape = false;
    [Tooltip("Forma del emisor cuando 'overrideShape' está activo.")]
    public ParticleSystemShapeType shapeType = ParticleSystemShapeType.Sphere;
    [Tooltip("Tope de partículas vivas (main.maxParticles). 0 = no sobreescribir el default del arquetipo.")]
    public int maxParticles = 0;
}