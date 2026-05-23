using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonolithController : NetworkBehaviour
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

    [Header("Configuración de Hundimiento")]
    public float sinkDepth = 5f;
    public float sinkDuration = 2f;

    void Awake()
    {
        data = new MonolithData(id, level, runeChallenge);
    }

    public void ServerInitialize()
    {
        if (!IsServer) return; 
        
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
        if (!IsServer) return;

        if (spells.Contains(spellName)) spells.Remove(spellName);
        
        if (spells.Count == 0) TriggerSinkEffectClientRpc();
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

            case Elements.Nature:
                return Elements.Thunder;

            default:
                return original;
        }

    }

    public void PopulateSpells()
    {
        spells.Clear();
        System.Array allElements = System.Enum.GetValues(typeof(Elements));
        int randomElementIndex = Random.Range(1, allElements.Length);
        Elements targetElement = (Elements)allElements.GetValue(randomElementIndex);
        targetElement = GetMappedElement(targetElement);

        Debug.Log($"[Monolito] Elemento Principal Elegido: {targetElement}");
        System.Array allTiers = System.Enum.GetValues(typeof(SpellTiers));

        foreach (SpellTiers currentTier in allTiers)
        {
            List<SpellData> spellsInThisTier = new List<SpellData>();
            
            foreach (SpellData sData in allSpellData)
            {
                if (sData == null) continue;

                if (sData.elementType == targetElement && sData.spellTier == currentTier) spellsInThisTier.Add(sData);
            }
            
            if (spellsInThisTier.Count > 0)
            {
                int randomSpellIndex = Random.Range(0, spellsInThisTier.Count);
                SpellData selectedSpellData = spellsInThisTier[randomSpellIndex];
                Spell matchingSpell = allSpells.Find(s => s.elementType == selectedSpellData.elementType /* Agrega aquí más condiciones si necesitas vincularlos exacto */);
                
                if (matchingSpell != null)
                {
                    spells.Add(matchingSpell);
                    Debug.Log($"[Monolito] Agregado - Tier: {currentTier} | Hechizo: {selectedSpellData.runeString}");
                }
            }
            else
            {
                Debug.LogWarning($"[Monolito] No hay hechizos de {targetElement} en el Tier {currentTier}. Saltando...");
            }
        }
    }
    
    [ClientRpc]
    private void TriggerSinkEffectClientRpc()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        StartCoroutine(SinkRoutine());
    }

    private IEnumerator SinkRoutine()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + (Vector3.down * sinkDepth);
        float elapsedTime = 0f;

        while (elapsedTime < sinkDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / sinkDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        
        if (IsServer) GetComponent<NetworkObject>().Despawn(true);
    }
}
