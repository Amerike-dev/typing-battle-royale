using System.Collections.Generic;
using System.Linq;

public class PlayerInventory
{
    // Cantidad de casteos exitosos del tier anterior necesarios para desbloquear el siguiente.
    public const int CastsToUnlockNextTier = 3;

    private List<Spell> _spells;
    private PlayerController _owner;

    // Casteos exitosos acumulados por tier (progresión de desbloqueo).
    private readonly Dictionary<SpellTiers, int> _castsByTier = new Dictionary<SpellTiers, int>();

    public PlayerInventory(PlayerController owner)
    {
        _spells = new List<Spell>();
        _owner = owner;
    }

    /// <summary>Registra un casteo exitoso de un hechizo del tier indicado (suma a la progresión).</summary>
    public void RegisterSpellCast(SpellTiers tier)
    {
        _castsByTier.TryGetValue(tier, out int count);
        _castsByTier[tier] = count + 1;
    }

    private int CastCount(SpellTiers tier)
    {
        _castsByTier.TryGetValue(tier, out int count);
        return count;
    }

    /// <summary>
    /// Tier máximo que el jugador puede usar. Arranca en T1. Castear N hechizos T1 desbloquea
    /// TODOS los T2; castear N hechizos T2 desbloquea los T3 (N = CastsToUnlockNextTier).
    /// </summary>
    public SpellTiers UnlockedTier
    {
        get
        {
            SpellTiers tier = SpellTiers.T1;
            if (CastCount(SpellTiers.T1) >= CastsToUnlockNextTier) tier = SpellTiers.T2;
            if (tier == SpellTiers.T2 && CastCount(SpellTiers.T2) >= CastsToUnlockNextTier) tier = SpellTiers.T3;
            return tier;
        }
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