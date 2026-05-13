using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpellCatalog", menuName = "Scriptable Objects/Spell Catalog")]
public class SpellCatalog : ScriptableObject
{
    public Spell[] spells;

    static SpellCatalog _instance;
    Dictionary<string, int> _runeToIndex;

    public static SpellCatalog Instance
    {
        get
        {
            if (_instance == null) _instance = Resources.Load<SpellCatalog>("SpellCatalog");
            return _instance;
        }
    }

    void OnEnable()
    {
        BuildIndex();
    }

    void BuildIndex()
    {
        _runeToIndex = new Dictionary<string, int>(spells != null ? spells.Length : 0);
        if (spells == null) return;
        for (int i = 0; i < spells.Length; i++)
        {
            var s = spells[i];
            if (s == null || string.IsNullOrEmpty(s.runeString)) continue;
            var key = s.runeString.ToLowerInvariant();
            if (!_runeToIndex.ContainsKey(key)) _runeToIndex.Add(key, i);
        }
    }

    public int IndexOf(Spell spell)
    {
        if (spell == null || spells == null) return -1;
        for (int i = 0; i < spells.Length; i++)
            if (spells[i] == spell) return i;
        return -1;
    }

    public Spell Get(int id)
    {
        if (spells == null || id < 0 || id >= spells.Length) return null;
        return spells[id];
    }

    public Spell GetByRune(string rune)
    {
        if (string.IsNullOrEmpty(rune) || spells == null) return null;
        if (_runeToIndex == null) BuildIndex();
        return _runeToIndex.TryGetValue(rune.ToLowerInvariant(), out int idx) ? spells[idx] : null;
    }
}
