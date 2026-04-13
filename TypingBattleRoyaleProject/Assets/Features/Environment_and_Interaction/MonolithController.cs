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
    [Header("Datos Elementos")]
    public List<Spell> allSpells = new List<Spell>();
    public List<SpellData> allSpellData = new List<SpellData>();


    void Awake()
    {
        data = new MonolithData(id, level, runeChallenge);
        PopulateSpells();

        GetComponent<MonolithView>().SetData(data);
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
        for (int i = 0; i < spells.Count; i++)
        {
            spells.Remove(spellName);
        }
    }
    Elements GetMappedElement(Elements original)
    {

        switch (original)
        {
            case Elements.Wind:
                return Elements.Thunder;

            case Elements.Water:
                return Elements.Water;

            case Elements.Earth:
                return Elements.Fire;

            case Elements.Arcane:
                return Elements.Thunder;

            default:
                return original;
        }

    }

    public void PopulateSpells()
    {
        spells.Clear();

        int randomIndex = Random.Range(1, System.Enum.GetValues(typeof(Elements)).Length);
        Elements randomElement = (Elements)randomIndex;

        for (int i = 0; i < allSpells.Count; i++)
        {
            if (allSpells[i].elementType == randomElement)
            {
                spells.Add(allSpells[i]);
            }
        }

        for (int i = 0; i < spells.Count; i++)
        {
            Spell currentSpell = spells[i];

            Elements mappedElement = GetMappedElement(currentSpell.elementType);

            int randomTier = Random.Range(0, System.Enum.GetValues(typeof(SpellTiers)).Length);
            SpellTiers selectedTier = (SpellTiers)randomTier;

            for (int j = 0; j < allSpellData.Count; j++)
            {
                SpellData data = allSpellData[j];

                if (data.elementType == mappedElement && data.spellTier == selectedTier)
                {
                    Debug.Log("Elemento " + randomElement + "  Spell " + data.runeString + "Tier" + selectedTier);
                }
            }
        }
    }
}
