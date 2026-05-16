using UnityEngine;

public class DamageCalculator
{
    private PlayerController _playerController;

    public DamageCalculator(PlayerController targetPlayer)
    {
        _playerController = targetPlayer;
    }

    public void CalculateDamage(SpellData castedSpell, TypingStats stats, ulong attackerId)
    {
        float accuracy = stats.GetAccuracy();
        float multiplier = TypingStats.GetDamageBonusMultiplier(accuracy);
        float finalDamage = castedSpell.baseDamage * multiplier;

        _playerController.stats.TakeDamage(finalDamage, attackerId);
    }
}