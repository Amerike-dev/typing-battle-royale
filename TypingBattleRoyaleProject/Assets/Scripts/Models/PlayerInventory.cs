using System.Collections.Generic;
using System.Linq;

public class PlayerInventory
{
    private List<Spell> _spells;
    private PlayerController _owner;

    public PlayerInventory(PlayerController owner)
    {
        _spells = new List<Spell>();
        _owner = owner;
    }

    public void AddSpell(Spell newSpell)
    {
        if (newSpell == null) return;

        if (!_spells.Contains(newSpell))
        {
            _spells.Add(newSpell);
            _owner.UpdateDebugList(_spells);
        }
    }
    
    public bool HasSpell(string spellName)
    {
        return _spells.Any(s => s.spellName == spellName);
    }

    public IReadOnlyList<Spell> GetUnlockedSpells()
    {
        return _spells.AsReadOnly();
    }

    public IEnumerable<Spell> GetSpellsByTier(SpellTiers tier)
    {
        return _spells.Where(spell => spell.tier == tier);
    }
}