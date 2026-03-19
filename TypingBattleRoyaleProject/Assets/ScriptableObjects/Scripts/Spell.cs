using UnityEngine;

public enum Elements { None, Fire, Water, Earth, Wind, Arcane, Thunder, Dark, Light}
[CreateAssetMenu(fileName = "Spell", menuName = "Wizard Stuff/Spell")]

public class Spell : ScriptableObject
{
    public string spellName;
    public Elements elementType;

    public float duration;
    public bool loop = true;
    public float startSpeed=2f;
    public float startSize=1f;
    public int emissionRate = 50;
    public float shapeRadius = 2.0f;
    public Material materialVFX; 
}