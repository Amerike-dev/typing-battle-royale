using UnityEngine;

public class DamageCalculator
{
    private PlayerController _playerController;

    public DamageCalculator(PlayerController targetPlayer)
    {
        _playerController = targetPlayer;
    }

    public void CalculateDamage(SpellData castedSpell)
    {
        _playerController.currentHealth -= castedSpell.baseDamage;
    }
}