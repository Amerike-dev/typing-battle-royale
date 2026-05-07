using UnityEngine;

[CreateAssetMenu(fileName = "SpellData", menuName = "Scriptable Objects/SpellData")]
public class SpellData : ScriptableObject
{
    public SpellTiers spellTier;
    public float baseDamage;
    public GameObject particlePrefab;
    public string runeString;
    public ParticleSystem particleVFX;
    public Elements elementType;
}