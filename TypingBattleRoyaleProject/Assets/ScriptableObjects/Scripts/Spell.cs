using UnityEngine;

public enum Elements { None = 0, Fire = 1, Water = 2, Earth = 3, Wind = 4, Nature = 5, Thunder = 6, Dark, Light, Ice, Lava}
public enum SpellTiers { TierOne, TierTwo, TierThree }
public enum SpellTypes {Projectile, Movility, Summon, Buff, Debuff, Aura, AOE, Weapon};
public enum StatusEffects {None, Slow, Freeze, Root, Poison}
[CreateAssetMenu(fileName = "Spell", menuName = "Scriptable Objects/Spell")]

public class Spell : ScriptableObject
{
    [Header("General Info")]
    public string spellName;
    public Elements elementType;
    public string description;
    [Header("Characteristics")]
    public SpellTypes[] spellTypes;
    public float damage;
    public StatusEffects debuff;
    public float range;
    public float speed;
    public int uses;
    public float duration;
    [Header("SFX")]
    public float particleLifeDuration;
    public bool loop = true;
    public float startSpeed=2f;
    public float startSize=1f;
    public int emissionRate = 50;
    public float shapeRadius = 2.0f;
    public Material materialVFX; 
}