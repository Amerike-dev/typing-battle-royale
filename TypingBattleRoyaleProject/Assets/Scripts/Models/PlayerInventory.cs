using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory
{
    //Gestiona una lista de hechizos recolectados.
    private List<Spell> _spells;

    public void AddSpell(Spell newSpeel)
    {
        _spells.Add(newSpeel);
    }
}
