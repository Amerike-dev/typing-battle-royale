using UnityEngine;

public enum Elements { None, Fire, Water, Earth, Wind, Arcane, Plasma, Dark, Light}
[CreateAssetMenu(fileName = "Spell", menuName = "Wizard Stuff/Spell")]

public class Spell : ScriptableObject
{
    public string spellName;
    public Elements elementType;
}
