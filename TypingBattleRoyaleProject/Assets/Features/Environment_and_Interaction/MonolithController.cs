using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MonolithController : MonoBehaviour
{
    [Header("Intanciar monolito")]
    public MonolithData data;
    public string id;
    public int level;
    public string runeChallenge;

    [Header("Revision monolito")]
    public List<string> idPlayers = new List<string>();
    public List<Spell> spells = new List<Spell>();

    void Awake()
    {
        data = new MonolithData(id, level, runeChallenge);
        PopulateSpells();
    }

    public void AddIdPlayer(string id)
    {
        idPlayers.Add(id);
    }
    
    public bool IdPlayerExist(string id)
    {
        if (!idPlayers.Contains(id))
        {
            AddIdPlayer(id);
            return false;
        }
        else
        {
            Debug.Log("Este player ya agarro un hechizo");
            return true;
        }
    }

    public void RemoveSpellData(Spell spellName)
    {
        for(int i = 0; i < spells.Count; i++)
        {
            spells.Remove(spellName);
        }
    }

    public void PopulateSpells()
    {
        //To do: Agregar llenado de spells
    }
}
