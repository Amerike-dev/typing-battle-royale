using System.Collections.Generic;
using System.Linq;

public class PlayerInventory
{
    private List<SpellData> _spells;

    public PlayerInventory()
    {
        _spells = new List<SpellData>();
    }

    public void AddSpell(SpellData newSpell)
    {
        if (newSpell == null) return;

        if (!_spells.Contains(newSpell))
        {
            _spells.Add(newSpell);
        }
    }

    public IReadOnlyList<SpellData> GetUnlockedSpells()
    {
        return _spells.AsReadOnly();
    }

    public IEnumerable<SpellData> GetSpellsByTier(SpellTiers tier)
    {
        return _spells.Where(spell => spell.spellTier == tier);
    }
}