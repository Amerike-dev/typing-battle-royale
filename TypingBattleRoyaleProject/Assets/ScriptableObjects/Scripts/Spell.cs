using UnityEngine;

public enum Elements { None = 0, Fire = 1, Water = 2, Earth = 3, Wind = 4, Arcane = 5, Thunder = 6, Dark, Light}
[CreateAssetMenu(fileName = "Spell", menuName = "Wizard Stuff/Spell")]

public class Spell : ScriptableObject
{
    [Header("Dependencias")]
    [Header("Información")]
    [Header("Estadisticas")]
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